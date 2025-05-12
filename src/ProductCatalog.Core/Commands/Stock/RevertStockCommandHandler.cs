using MediatR;
using Microsoft.Extensions.Logging;
using ProductCatalog.Core.Interfaces;
using SharedKernel.Commands.Stock;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Core.Commands.Stock
{
    /// <summary>
    /// SharedKernel'dan gelen ürün stok geri ekleme komutunu işleyen handler
    /// </summary>
    public class RevertStockCommandHandler : IRequestHandler<RevertStockCommand, RevertStockResult>
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<RevertStockCommandHandler> _logger;

        public RevertStockCommandHandler(
            IProductRepository productRepository,
            ILogger<RevertStockCommandHandler> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<RevertStockResult> Handle(RevertStockCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Ürün stoğu geri ekleniyor. ProductId: {ProductId}, Miktar: {Quantity}",
                    request.ProductId, request.Quantity);

                // Ürünü bul
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Stok geri ekleme işlemi başarısız: Ürün bulunamadı. ProductId: {ProductId}",
                        request.ProductId);
                    
                    return RevertStockResult.Failure(
                        request.ProductId,
                        $"Ürün bulunamadı. ID: {request.ProductId}");
                }

                // Stok miktarını geri ekle
                product.StockQuantity += request.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                
                // Veritabanını güncelle
                await _productRepository.UpdateAsync(product);
                
                _logger.LogInformation("Ürün stoğu başarıyla geri eklendi. ProductId: {ProductId}, Miktar: {Quantity}, Yeni Stok: {NewStock}",
                    product.Id, request.Quantity, product.StockQuantity);

                return RevertStockResult.Success(
                    product.Id,
                    request.Quantity,
                    product.StockQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok geri ekleme işlemi sırasında hata oluştu. ProductId: {ProductId}, Miktar: {Quantity}",
                    request.ProductId, request.Quantity);
                
                return RevertStockResult.Failure(
                    request.ProductId,
                    $"Stok geri ekleme işlemi sırasında hata: {ex.Message}");
            }
        }
    }
}
