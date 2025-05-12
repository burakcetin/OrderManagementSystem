using MediatR;
using Microsoft.Extensions.Logging;
using ProductCatalog.Core.Entities;
using ProductCatalog.Core.Interfaces;
using SharedKernel.Commands.Stock;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Core.Commands.Stock
{
    /// <summary>
    /// SharedKernel'dan gelen ürün stok düşme komutunu işleyen handler
    /// </summary>
    public class ReduceStockCommandHandler : IRequestHandler<ReduceStockCommand, ReduceStockResult>
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ReduceStockCommandHandler> _logger;

        public ReduceStockCommandHandler(
            IProductRepository productRepository,
            ILogger<ReduceStockCommandHandler> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ReduceStockResult> Handle(ReduceStockCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Ürün stoğu düşürülüyor. ProductId: {ProductId}, Miktar: {Quantity}",
                    request.ProductId, request.Quantity);

                // Ürünü bul
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Stok düşme işlemi başarısız: Ürün bulunamadı. ProductId: {ProductId}",
                        request.ProductId);
                    
                    return ReduceStockResult.Failure(
                        request.ProductId,
                        $"Ürün bulunamadı. ID: {request.ProductId}");
                }

                // Yeterli stok var mı kontrol et
                if (product.StockQuantity < request.Quantity)
                {
                    _logger.LogWarning("Stok düşme işlemi başarısız: Yetersiz stok. ProductId: {ProductId}, Mevcut: {Available}, İstenen: {Requested}",
                        request.ProductId, product.StockQuantity, request.Quantity);
                    
                    return ReduceStockResult.Failure(
                        request.ProductId,
                        $"Yetersiz stok. Mevcut: {product.StockQuantity}, İstenen: {request.Quantity}");
                }

                // Stok miktarını düşür
                product.StockQuantity -= request.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                
                // Veritabanını güncelle
                await _productRepository.UpdateAsync(product);
                
                _logger.LogInformation("Ürün stoğu başarıyla düşürüldü. ProductId: {ProductId}, Miktar: {Quantity}, Yeni Stok: {NewStock}",
                    product.Id, request.Quantity, product.StockQuantity);

                return ReduceStockResult.Success(
                    product.Id,
                    request.Quantity,
                    product.StockQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok düşme işlemi sırasında hata oluştu. ProductId: {ProductId}, Miktar: {Quantity}",
                    request.ProductId, request.Quantity);
                
                return ReduceStockResult.Failure(
                    request.ProductId,
                    $"Stok işlemi sırasında hata: {ex.Message}");
            }
        }
    }
}
