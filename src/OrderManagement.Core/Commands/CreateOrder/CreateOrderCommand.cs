using MediatR;
using OrderManagement.Core.Models;
using System;

namespace OrderManagement.Core.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<OrderDto>
    {
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
    }
}
