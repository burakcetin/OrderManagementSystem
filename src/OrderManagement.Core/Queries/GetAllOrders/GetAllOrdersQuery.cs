using MediatR;
using OrderManagement.Core.Models;

namespace OrderManagement.Core.Queries.GetAllOrders
{
    public class GetAllOrdersQuery : IRequest<List<OrderDto>>
    {
    }
}
