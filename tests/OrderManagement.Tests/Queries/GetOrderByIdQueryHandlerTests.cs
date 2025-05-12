using Moq;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using OrderManagement.Core.Queries.GetOrderById;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderManagement.Tests.Queries
{
    public class GetOrderByIdQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly GetOrderByIdQueryHandler _handler;

        public GetOrderByIdQueryHandlerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _handler = new GetOrderByIdQueryHandler(_mockOrderRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOrderDto_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                ProductName = "Test Product",
                Price = 100.0m,
                Quantity = 2,
                Status = OrderStatus.Created,
                CreatedAt = DateTime.UtcNow,
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                ShippingAddress = "Test Address"
            };

            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            var query = new GetOrderByIdQuery { OrderId = orderId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.Equal(order.ProductName, result.ProductName);
            Assert.Equal(order.Price, result.Price);
            Assert.Equal(order.Quantity, result.Quantity);
            Assert.Equal(order.Status, result.Status);
            
            _mockOrderRepository.Verify(repo => repo.GetByIdAsync(orderId), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
                .ReturnsAsync((Order)null);

            var query = new GetOrderByIdQuery { OrderId = orderId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
            
            _mockOrderRepository.Verify(repo => repo.GetByIdAsync(orderId), Times.Once);
        }
    }
}
