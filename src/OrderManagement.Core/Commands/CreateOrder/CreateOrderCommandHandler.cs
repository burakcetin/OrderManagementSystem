using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using OrderManagement.Core.Models;
using OrderManagement.Core.Messages;
using Rebus.Bus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderManagement.Core.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
    {
        private readonly IBus _bus;
        private readonly ILogger<CreateOrderCommandHandler> _logger;
        private readonly IOrderRepository _orderRepository;

        public CreateOrderCommandHandler(
            IBus bus,
            IOrderRepository orderRepository,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _bus = bus;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
           
                var orderId = Guid.NewGuid();
                var creationTime = DateTime.UtcNow;
                
                var order = new Order
                {
                    Id = orderId,
                    ProductId = request.ProductId,
                    ProductName = request.ProductName,
                    Price = request.Price,
                    Quantity = request.Quantity,
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    ShippingAddress = request.ShippingAddress,
                    Status = OrderStatus.Pending,  
                    CreatedAt = creationTime,
                    UpdatedAt = creationTime
                };

            
                await _orderRepository.AddAsync(order);
                _logger.LogInformation("Sipariş veritabanına kaydedildi. OrderId: {OrderId}", orderId);
                
                // Mesaj hazırla ve gönder
                var message = new OrderCreatedMessage
                {
                    OrderId = orderId,
                    ProductName = request.ProductName,
                    Price = request.Price,
                    Quantity = request.Quantity,
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    ShippingAddress = request.ShippingAddress,
                    ProductId = request.ProductId, 
                    CreatedAt = creationTime
                };

                await _bus.Send(message);
                _logger.LogInformation("Sipariş mesajı Rebus ile gönderildi. OrderId: {OrderId}", orderId);
               
                return new OrderDto
                {
                    Id = orderId,
                    ProductId = order.ProductId,
                    ProductName = order.ProductName,
                    Price = order.Price,
                    Quantity = order.Quantity,
                    Status = order.Status, // Pending durumunu dön
                    CreatedAt = order.CreatedAt,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    ShippingAddress = order.ShippingAddress
                };

        }
    }
}
