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
    /// Ürün stoğunu geri ekleyen komut işleyici (ProductService aracılığıyla)
    /// </summary>
    public class RevertStockCommandHandler : IRequestHandler<RevertStockCommand, RevertStockResult>
    {
        private readonly IProductService _productService;
        private readonly ILogger<RevertStockCommandHandler> _logger;

        public RevertStockCommandHandler(
            IProductService productService,
            ILogger<RevertStockCommandHandler> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public async Task<RevertStockResult> Handle(RevertStockCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Ürün stoğu geri ekleme isteği alındı. ProductId: {ProductId}, Miktar: {Quantity}",
                    request.ProductId, request.Quantity);

                // ProductService üzerinden işlemi gerçekleştir
                var result = await _productService.RestoreStockAsync(request.ProductId, request.Quantity);

                if (!result.Success)
                {
                    _logger.LogWarning("Stok geri ekleme işlemi başarısız oldu. ProductId: {ProductId}, Hata: {Error}",
                        request.ProductId, result.ErrorMessage);
                    
                    return RevertStockResult.Failure(
                        request.ProductId,
                        result.ErrorMessage);
                }

                _logger.LogInformation("Stok geri ekleme işlemi başarılı. ProductId: {ProductId}, Miktar: {Quantity}, YeniStok: {NewStock}",
                    request.ProductId, request.Quantity, result.RemainingStock);
                
                return RevertStockResult.Success(
                    request.ProductId,
                    request.Quantity,
                    result.RemainingStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok geri ekleme işleminde beklenmeyen hata. ProductId: {ProductId}", request.ProductId);
                
                return RevertStockResult.Failure(
                    request.ProductId,
                    $"Beklenmeyen hata: {ex.Message}");
            }
        }
    }
}
