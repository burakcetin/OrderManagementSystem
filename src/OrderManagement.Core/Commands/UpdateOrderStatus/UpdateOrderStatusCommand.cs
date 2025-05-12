using MediatR;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Models;

namespace OrderManagement.Core.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommand : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
    }
}
