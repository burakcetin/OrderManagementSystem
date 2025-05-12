using MediatR;
using Microsoft.Extensions.Logging;
using ProductCatalog.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Core.Commands.UpdateProduct
{
    /// <summary>
    /// Ürün güncelleme komutunu işleyen handler
    /// </summary>
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdateProductCommandResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            ILogger<UpdateProductCommandHandler> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        /// <summary>
        /// Komutu işler
        /// </summary>
        public async Task<UpdateProductCommandResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Ürün güncelleme işlemi başlatılıyor. ProductId: {ProductId}", request.ProductId);

                var product = await _productRepository.GetByIdAsync(request.ProductId);

                if (product == null)
                {
                    _logger.LogWarning("Ürün güncellenemedi: Ürün bulunamadı. ProductId: {ProductId}", request.ProductId);
                    return UpdateProductCommandResponse.Fail($"Ürün bulunamadı: {request.ProductId}");
                }

                product.Name = request.Name;
                product.Description = request.Description;
                product.Price = request.Price;
                product.Stock = request.Stock;
                product.Category = request.Category;
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.UpdateAsync(product);

                _logger.LogInformation("Ürün başarıyla güncellendi. ProductId: {ProductId}, Yeni Adı: {ProductName}", 
                    product.Id, product.Name);

                var result = new UpdateProductCommandResult
                {
                    IsSuccess = true,
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Stock = product.Stock
                };

                return UpdateProductCommandResponse.Success(result, "Ürün başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün güncelleme işleminde hata. ProductId: {ProductId}", request.ProductId);
                return UpdateProductCommandResponse.Fail($"Ürün güncelleme işleminde hata: {ex.Message}");
            }
        }
    }
}
