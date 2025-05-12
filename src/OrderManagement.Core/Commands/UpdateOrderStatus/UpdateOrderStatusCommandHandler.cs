using MediatR;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using OrderManagement.Core.Models;

namespace OrderManagement.Core.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;

        public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                return null;

            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            return MapToDto(order);
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                ProductId = order.ProductId,
                ProductName = order.ProductName,
                Price = order.Price,
                Quantity = order.Quantity,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                ShippingAddress = order.ShippingAddress
            };
        }
    }
}
