using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderManagement.Core.Commands.UpdateOrderStatus;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Queries.GetAllOrders;
using OrderManagement.Core.Queries.GetOrderById;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using OrderManagement.Core.Models;
using Rebus.Exceptions;
using OrderManagement.Core.Commands.CreateOrder;

namespace OrderManagement.API.Controllers
{
    /// <summary>
    /// API endpoints for managing orders
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdersController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        /// <returns>List of all orders</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OrderDto>>> GetAll()
        {

            var query = new GetAllOrdersQuery();
            var orders = await _mediator.Send(query);

            return Ok(orders);

        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> GetById(Guid id)
        {

            var query = new GetOrderByIdQuery { OrderId = id };
            var order = await _mediator.Send(query);

            if (order == null)
                return NotFound($"Order with ID {id} not found");

            return Ok(order);

        }

        /// <summary>
        /// Get order status by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order status information</returns>
        [HttpGet("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetOrderStatus(Guid id)
        {
            var query = new GetOrderByIdQuery { OrderId = id };
            var order = await _mediator.Send(query);

            if (order == null)
                return NotFound($"Order with ID {id} not found");

            var statusInfo = new
            {
                OrderId = order.Id,
                Status = order.Status,
                StatusDescription = GetStatusDescription(order.Status),
                LastUpdated = order.UpdatedAt
            };

            return Ok(statusInfo);

        }

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="statusUpdate">New status data</param>
        /// <returns>The updated order</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, [FromBody] OrderStatusUpdateDto statusUpdate)
        {

            var command = new UpdateOrderStatusCommand
            {
                OrderId = id,
                Status = statusUpdate.Status
            };

            var order = await _mediator.Send(command);

            if (order == null)
                return NotFound($"Order with ID {id} not found");

            return Ok(order);

        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="command">Order creation data</param>
        /// <returns>The created order</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)] // 202 Accepted kullanıyoruz
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderCommand command)
        {

            var createdOrder = await _mediator.Send(command);

            return AcceptedAtAction(
                nameof(GetById),
                new { id = createdOrder.Id },
                createdOrder);
        }

        /// <summary>
        /// Gets the status description based on order status
        /// </summary>
        private string GetStatusDescription(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Sipariş alındı ve işlemeye başladı",
                OrderStatus.Created => "Sipariş işlemeye alındı",
                OrderStatus.Processing => "Siparişiniz işleniyor",
                OrderStatus.Confirmed => "Siparişiniz onaylandı",
                OrderStatus.Shipped => "Siparişiniz kargoya verildi",
                OrderStatus.Delivered => "Siparişiniz teslim edildi",
                OrderStatus.Cancelled => "Siparişiniz iptal edildi",
                OrderStatus.Failed => "Siparişinizde bir hata oluştu",
                OrderStatus.Returned => "Siparişiniz iade edildi",
                _ => "Bilinmeyen durum"
            };
        }
    }

    /// <summary>
    /// Data transfer object for updating order status
    /// </summary>
    public class OrderStatusUpdateDto
    {
        /// <summary>
        /// New status value
        /// </summary>
        public OrderStatus Status { get; set; }
    }
}