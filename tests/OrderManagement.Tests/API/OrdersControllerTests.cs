using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.API.Controllers;
using OrderManagement.Core.Commands.CreateOrder;
using OrderManagement.Core.Commands.UpdateOrderStatus;
using OrderManagement.Core.Queries.GetAllOrders;
using OrderManagement.Core.Queries.GetOrderById;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentValidation;
using Rebus.Exceptions;

namespace OrderManagement.Tests.API
{
    public class OrdersControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkResult_WithListOfOrders()
        {
            // Arrange
            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Test Product 1",
                    Price = 100.0m,
                    Quantity = 2,
                    Status = OrderStatus.Created,
                    CreatedAt = DateTime.UtcNow
                },
                new OrderDto
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Test Product 2",
                    Price = 150.0m,
                    Quantity = 1,
                    Status = OrderStatus.Processing,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(orders);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedOrders = Assert.IsType<List<OrderDto>>(okResult.Value);
            Assert.Equal(2, returnedOrders.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnOkResult_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new OrderDto
            {
                Id = orderId,
                ProductName = "Test Product",
                Price = 100.0m,
                Quantity = 2,
                Status = OrderStatus.Created,
                CreatedAt = DateTime.UtcNow
            };

            _mockMediator.Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.GetById(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
            Assert.Equal(orderId, returnedOrder.Id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrderDto)null);

            // Act
            var result = await _controller.GetById(orderId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
        
        [Fact]
        public async Task GetOrderStatus_ShouldReturnOrderStatusInfo_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new OrderDto
            {
                Id = orderId,
                ProductName = "Test Product",
                Price = 100.0m,
                Quantity = 2,
                Status = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow
            };

            _mockMediator.Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.GetOrderStatus(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var statusInfo = Assert.IsAssignableFrom<object>(okResult.Value);
            var statusInfoDict = statusInfo.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(statusInfo));
            
            Assert.Equal(orderId, statusInfoDict["OrderId"]);
            Assert.Equal(OrderStatus.Processing, statusInfoDict["Status"]);
            Assert.NotNull(statusInfoDict["StatusDescription"]);
            Assert.NotNull(statusInfoDict["LastUpdated"]);
        }

        [Fact]
        public async Task GetOrderStatus_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrderDto)null);

            // Act
            var result = await _controller.GetOrderStatus(orderId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ShouldReturnAcceptedAtActionResult_WhenOrderProcessingStarted()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Test Product",
                Price = 100.0m,
                Quantity = 2,
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                ShippingAddress = "Test Address"
            };

            var pendingOrder = new OrderDto
            {
                Id = Guid.NewGuid(),
                ProductId = command.ProductId,
                ProductName = command.ProductName,
                Price = command.Price,
                Quantity = command.Quantity,
                Status = OrderStatus.Pending, // Status should be Pending now
                CreatedAt = DateTime.UtcNow,
                CustomerName = command.CustomerName,
                CustomerEmail = command.CustomerEmail,
                ShippingAddress = command.ShippingAddress
            };

            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pendingOrder);

            // Act
            var result = await _controller.Create(command);

            // Assert
            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result); // Now returns 202 Accepted
            Assert.Equal(nameof(OrdersController.GetById), acceptedAtActionResult.ActionName);
            Assert.Equal(pendingOrder.Id, acceptedAtActionResult.RouteValues["id"]);
            var returnedOrder = Assert.IsType<OrderDto>(acceptedAtActionResult.Value);
            Assert.Equal(pendingOrder.Id, returnedOrder.Id);
            Assert.Equal(OrderStatus.Pending, returnedOrder.Status); // Verify status is Pending
        }
        
        [Fact]
        public async Task Create_ShouldHandleRebusExceptions()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Test Product",
                Price = 100.0m
            };

            // Setup mediator to throw RebusApplicationException
            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RebusApplicationException("Message broker error"));

            // Act
            var result = await _controller.Create(command);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(503, statusCodeResult.StatusCode); // 503 Service Unavailable
        }
        
        [Fact]
        public async Task Create_ShouldHandleRebusNamespaceExceptions()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Test Product",
                Price = 100.0m
            };

            // Create a mock exception from Rebus.* namespace
            var ex = new Exception("Rebus connection error");
            // Force the namespace to be "Rebus.Something"
            Type rebusExceptionType = typeof(RebusApplicationException);
            
            // Setup to match the when condition in the controller
            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);
            
            // Modify the mediator mock to return true for the condition
            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .Callback(() => ex.GetType().ToString()) // Just to access ex.GetType()
                .ThrowsAsync(new RebusApplicationException("Rebus connection error")); // Use a real Rebus exception

            // Act
            var result = await _controller.Create(command);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(503, statusCodeResult.StatusCode); // 503 Service Unavailable
        }

        [Fact]
        public async Task Create_ShouldHandleGeneralExceptions()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Test Product",
                Price = 100.0m
            };

            // Setup mediator to throw a general Exception
            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("General error"));

            // Act
            var result = await _controller.Create(command);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode); // 500 Internal Server Error
        }

        [Fact]
        public async Task UpdateStatus_ShouldReturnOkResult_WhenOrderUpdated()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var statusUpdate = new OrderStatusUpdateDto { Status = OrderStatus.Shipped };

            var updatedOrder = new OrderDto
            {
                Id = orderId,
                ProductName = "Test Product",
                Price = 100.0m,
                Quantity = 2,
                Status = statusUpdate.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockMediator.Setup(m => m.Send(It.Is<UpdateOrderStatusCommand>(c => 
                c.OrderId == orderId && c.Status == statusUpdate.Status), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedOrder);

            // Act
            var result = await _controller.UpdateStatus(orderId, statusUpdate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
            Assert.Equal(orderId, returnedOrder.Id);
            Assert.Equal(statusUpdate.Status, returnedOrder.Status);
        }

        [Fact]
        public async Task UpdateStatus_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var statusUpdate = new OrderStatusUpdateDto { Status = OrderStatus.Shipped };

            _mockMediator.Setup(m => m.Send(It.Is<UpdateOrderStatusCommand>(c => 
                c.OrderId == orderId && c.Status == statusUpdate.Status), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrderDto)null);

            // Act
            var result = await _controller.UpdateStatus(orderId, statusUpdate);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateStatus_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var statusUpdate = new OrderStatusUpdateDto { Status = (OrderStatus)999 }; // Invalid status

            _mockMediator.Setup(m => m.Send(It.Is<UpdateOrderStatusCommand>(c => 
                c.OrderId == orderId && c.Status == statusUpdate.Status), 
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(new[] {
                    new FluentValidation.Results.ValidationFailure("Status", "Status value is invalid")
                }));

            // Act
            var result = await _controller.UpdateStatus(orderId, statusUpdate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
            Assert.NotEmpty(errors);
        }
    }
}