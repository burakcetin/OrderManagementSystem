using Moq;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using OrderManagement.Core.Queries.GetAllOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderManagement.Tests.Queries
{
    public class GetAllOrdersQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly GetAllOrdersQueryHandler _handler;

        public GetAllOrdersQueryHandlerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _handler = new GetAllOrdersQueryHandler(_mockOrderRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOrderDtoList_WhenOrdersExist()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Test Product 1",
                    Price = 100.0m,
                    Quantity = 2,
                    Status = OrderStatus.Created,
                    CreatedAt = DateTime.UtcNow,
                    CustomerName = "Test Customer 1",
                    CustomerEmail = "test1@example.com",
                    ShippingAddress = "Test Address 1"
                },
                new Order
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Test Product 2",
                    Price = 150.0m,
                    Quantity = 1,
                    Status = OrderStatus.Processing,
                    CreatedAt = DateTime.UtcNow,
                    CustomerName = "Test Customer 2",
                    CustomerEmail = "test2@example.com",
                    ShippingAddress = "Test Address 2"
                }
            };

            _mockOrderRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(orders);

            // Act
            var result = await _handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(orders[0].Id, result[0].Id);
            Assert.Equal(orders[1].Id, result[1].Id);
            
            _mockOrderRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            _mockOrderRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            
            _mockOrderRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }
    }
}
