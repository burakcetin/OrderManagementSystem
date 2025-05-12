using MediatR;
using OrderManagement.Core.Models;

namespace OrderManagement.Core.Queries.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
    }
}
