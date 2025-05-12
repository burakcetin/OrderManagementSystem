using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Core.Services;
using SharedKernel.Commands.Stock;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderManagement.Core.Commands.Stock
{
    /// <summary>
    /// Ürün stoğunu düşüren komut işleyici (ProductService aracılığıyla)
    /// </summary>
    public class ReduceStockCommandHandler : IRequestHandler<ReduceStockCommand, ReduceStockResult>
    {
        private readonly IProductService _productService;
        private readonly ILogger<ReduceStockCommandHandler> _logger;

        public ReduceStockCommandHandler(
            IProductService productService,
            ILogger<ReduceStockCommandHandler> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public async Task<ReduceStockResult> Handle(ReduceStockCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Ürün stoğu düşme isteği alındı. ProductId: {ProductId}, Miktar: {Quantity}",
                    request.ProductId, request.Quantity);

                // ProductService üzerinden işlemi gerçekleştir
                var result = await _productService.ReduceStockAsync(request.ProductId, request.Quantity);

                if (!result.Success)
                {
                    _logger.LogWarning("Stok düşme işlemi başarısız oldu. ProductId: {ProductId}, Hata: {Error}",
                        request.ProductId, result.ErrorMessage);
                    
                    return ReduceStockResult.Failure(
                        request.ProductId,
                        result.ErrorMessage);
                }

                _logger.LogInformation("Stok düşme işlemi başarılı. ProductId: {ProductId}, Miktar: {Quantity}, YeniStok: {NewStock}",
                    request.ProductId, request.Quantity, result.RemainingStock);
                
                return ReduceStockResult.Success(
                    request.ProductId,
                    request.Quantity,
                    result.RemainingStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok düşme işleminde beklenmeyen hata. ProductId: {ProductId}", request.ProductId);
                
                return ReduceStockResult.Failure(
                    request.ProductId,
                    $"Beklenmeyen hata: {ex.Message}");
            }
        }
    }
}
