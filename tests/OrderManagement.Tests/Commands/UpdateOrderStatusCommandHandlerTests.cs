using Moq;
using OrderManagement.Core.Commands.UpdateOrderStatus;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderManagement.Tests.Commands
{
    public class UpdateOrderStatusCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly UpdateOrderStatusCommandHandler _handler;

        public UpdateOrderStatusCommandHandlerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _handler = new UpdateOrderStatusCommandHandler(_mockOrderRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnUpdatedOrderDto_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var newStatus = OrderStatus.Processing;

            var command = new UpdateOrderStatusCommand
            {
                OrderId = orderId,
                Status = newStatus
            };

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
            
            _mockOrderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.Equal(newStatus, result.Status);
            
            _mockOrderRepository.Verify(repo => repo.GetByIdAsync(orderId), Times.Once);
            _mockOrderRepository.Verify(repo => repo.UpdateAsync(It.Is<Order>(o => o.Status == newStatus)), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new UpdateOrderStatusCommand
            {
                OrderId = orderId,
                Status = OrderStatus.Processing
            };

            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result);
            _mockOrderRepository.Verify(repo => repo.GetByIdAsync(orderId), Times.Once);
            _mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Never);
        }
    }
}
