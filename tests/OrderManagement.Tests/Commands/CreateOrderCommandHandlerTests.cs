using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Core.Commands.CreateOrder;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using OrderManagement.Core.Models;
using OrderManagement.Core.Messages;
using Rebus.Bus;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderManagement.Tests.Commands
{
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IBus> _mockBus;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<ILogger<CreateOrderCommandHandler>> _mockLogger;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _mockBus = new Mock<IBus>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockLogger = new Mock<ILogger<CreateOrderCommandHandler>>();
            
            _handler = new CreateOrderCommandHandler(
                _mockBus.Object, 
                _mockOrderRepository.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ShouldSaveOrderAndSendMessage_WhenCommandIsValid()
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

            Order savedOrder = null;
            _mockOrderRepository.Setup(repo => repo.AddAsync(It.IsAny<Order>()))
                .Callback<Order>(order => savedOrder = order)
                .ReturnsAsync((Order order) => order);

            _mockBus.Setup(bus => bus.Send(It.IsAny<OrderCreatedMessage>(), null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.ProductName, result.ProductName);
            Assert.Equal(command.Price, result.Price);
            Assert.Equal(command.Quantity, result.Quantity);
            Assert.Equal(OrderStatus.Pending, result.Status); // Status should be Pending now
            
            // Verify repository was called
            _mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once);
            
            // Verify bus.Send was called with OrderCreatedMessage
            _mockBus.Verify(bus => bus.Send(It.IsAny<OrderCreatedMessage>(), null), Times.Once);
            
            // Verify that the order was saved with correct status
            Assert.NotNull(savedOrder);
            Assert.Equal(OrderStatus.Pending, savedOrder.Status);
        }
        
        [Fact]
        public async Task Handle_ShouldThrowException_WhenBusSendFails()
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

            _mockOrderRepository.Setup(repo => repo.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order order) => order);

            // Setup bus to throw exception
            _mockBus.Setup(bus => bus.Send(It.IsAny<OrderCreatedMessage>(), null))
                .ThrowsAsync(new Exception("Message sending failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(command, CancellationToken.None));
                
            // Verify repository was still called
            _mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once);
        }
    }
}
