using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedKernel.Commands.Stock;
using ProductCatalog.Core.Commands.UpdateProduct;
using ProductCatalog.Core.Models;
using ProductCatalog.Core.Queries.GetAllProducts;
using ProductCatalog.Core.Queries.GetProductById;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductCatalog.API.Controllers
{
    /// <summary>
    /// API endpoints for managing products
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <returns>List of all products</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] string category = null)
        {
            try
            {
                var query = new GetAllProductsQuery { Category = category };
                var products = await _mediator.Send(query);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDto>> GetById(Guid id)
        {
            try
            {
                var query = new GetProductByIdQuery { ProductId = id };
                var product = await _mediator.Send(query);
                
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product {id}");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Update product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="command">Update product command</param>
        /// <returns>Result of the product update operation</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UpdateProductCommandResult>> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
        {
            try
            {
                if (command.ProductId != id)
                {
                    command.ProductId = id;
                }
                
                _logger.LogInformation("Ürün güncelleme isteği alındı. ProductId: {ProductId}", command.ProductId);
                
                var response = await _mediator.Send(command);
                
                if (!response.Succeeded)
                {
                    if (response.Message?.Contains("bulunamadı") == true)
                    {
                        return NotFound(response.Message);
                    }
                    
                    return BadRequest(response.Message);
                }
                
                return Ok(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün güncelleme işleminde hata. ProductId: {ProductId}", id);
                return StatusCode(500, "An unexpected error occurred");
            }
        }
        
        /// <summary>
        /// Reduce product stock by quantity
        /// </summary>
        /// <param name="command">Reduce stock command</param>
        /// <returns>Result of the stock reduction operation</returns>
        [HttpPost("reduce-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ReduceStockResult>> ReduceStock([FromBody] ReduceStockCommand command)
        {
            try
            {
                _logger.LogInformation("Stok düşme isteği alındı. ProductId: {ProductId}, Quantity: {Quantity}", 
                    command.ProductId, command.Quantity);
                
                var result = await _mediator.Send(command);
                
                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage.Contains("bulunamadı"))
                    {
                        return NotFound(result.ErrorMessage);
                    }
                    
                    return BadRequest(result.ErrorMessage);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok düşme işleminde hata. ProductId: {ProductId}", command.ProductId);
                return StatusCode(500, "An unexpected error occurred");
            }
        }
        
        /// <summary>
        /// Revert product stock by quantity
        /// </summary>
        /// <param name="command">Revert stock command</param>
        /// <returns>Result of the stock revert operation</returns>
        [HttpPost("revert-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RevertStockResult>> RevertStock([FromBody] RevertStockCommand command)
        {
            try
            {
                _logger.LogInformation("Stok geri ekleme isteği alındı. ProductId: {ProductId}, Quantity: {Quantity}", 
                    command.ProductId, command.Quantity);
                
                var result = await _mediator.Send(command);
                
                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage.Contains("bulunamadı"))
                    {
                        return NotFound(result.ErrorMessage);
                    }
                    
                    return BadRequest(result.ErrorMessage);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok geri ekleme işleminde hata. ProductId: {ProductId}", command.ProductId);
                return StatusCode(500, "An unexpected error occurred");
            }
        }
    }
}
