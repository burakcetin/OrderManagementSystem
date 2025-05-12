using Microsoft.Extensions.Logging;
using OrderManagement.Core.Services;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure.Services
{
    /// <summary>
    /// Ürün servisini implemente eden sınıf
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        private readonly Random _random = new Random();

        public ProductService(
            IHttpClientFactory httpClientFactory,
            ILogger<ProductService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ProductCatalogApi");
            _logger = logger;
        }

        /// <summary>
        /// Ürün stoğunu azaltır (Product Microservice'e HTTP isteği gönderir)
        /// </summary>
        public async Task<ProductStockResult> ReduceStockAsync(Guid productId, int quantity)
        {
            try
            {
                _logger.LogInformation("Ürün stoğu azaltılıyor. ProductId: {ProductId}, Miktar: {Quantity}", 
                    productId, quantity);
                
                // Bu gerçek bir HTTP isteği yapma simülasyonu
                await Task.Delay(200);
                
                // %20 şansla hata fırlat (test için)
                if (_random.Next(0, 5) == 0)
                {
                    _logger.LogError("Ürün stoğu azaltılamadı. ProductId: {ProductId}", productId);
                    return new ProductStockResult
                    {
                        Success = false,
                        ErrorMessage = "Stok işlemi başarısız. Yeterli stok yok veya sistemde hata oluştu."
                    };
                }
                
                // Başarılı sonuç
                _logger.LogInformation("Ürün stoğu başarıyla azaltıldı. ProductId: {ProductId}", productId);
                return new ProductStockResult
                {
                    Success = true,
                    RemainingStock = 100 - quantity // Simüle edilmiş miktar
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün stoğu azaltma işlemi sırasında hata. ProductId: {ProductId}", productId);
                return new ProductStockResult
                {
                    Success = false,
                    ErrorMessage = $"Hata: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Ürün stoğunu geri yükler (Product Microservice'e HTTP isteği gönderir)
        /// </summary>
        public async Task<ProductStockResult> RestoreStockAsync(Guid productId, int quantity)
        {
            try
            {
                _logger.LogInformation("Ürün stoğu geri yükleniyor. ProductId: {ProductId}, Miktar: {Quantity}", 
                    productId, quantity);
                
                // Bu gerçek bir HTTP isteği yapma simülasyonu
                await Task.Delay(200);
                
                // Başarılı sonuç
                _logger.LogInformation("Ürün stoğu başarıyla geri yüklendi. ProductId: {ProductId}", productId);
                return new ProductStockResult
                {
                    Success = true,
                    RemainingStock = 100 // Simüle edilmiş miktar
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün stoğu geri yükleme işlemi sırasında hata. ProductId: {ProductId}", productId);
                return new ProductStockResult
                {
                    Success = false,
                    ErrorMessage = $"Hata: {ex.Message}"
                };
            }
        }
    }
}
