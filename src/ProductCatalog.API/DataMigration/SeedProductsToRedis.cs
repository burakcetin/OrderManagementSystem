using Microsoft.Extensions.Logging;
using ProductCatalog.Core.Entities;
using SharedKernel.Data.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductCatalog.API.DataMigration
{
    /// <summary>
    /// Redis veritabanını örnek ürünlerle başlatmak için yardımcı sınıf
    /// </summary>
    public class SeedProductsToRedis
    {
        private readonly IRedisRepository<Product> _redisRepository;
        private readonly ILogger<SeedProductsToRedis> _logger;

        public SeedProductsToRedis(
            IRedisRepository<Product> redisRepository,
            ILogger<SeedProductsToRedis> logger)
        {
            _redisRepository = redisRepository;
            _logger = logger;
        }

        /// <summary>
        /// Redis'e örnek ürünleri ekler
        /// </summary>
        public async Task SeedProductsAsync()
        {
            try
            {
                // Önce veritabanında herhangi bir ürün var mı kontrol et
                var existingProducts = await _redisRepository.GetAllAsync();
                if (existingProducts.Any())
                {
                    _logger.LogInformation("Redis'te zaten {Count} ürün var, veri eklemesi yapılmayacak", existingProducts.Count());
                    return;
                }

                _logger.LogInformation("Redis'e örnek ürünler ekleniyor");

                // Örnek ürünler
                var products = new List<Product>
                {
                    new Product
                    {
                        Id = Guid.Parse("e3d2bfd9-10e6-4c7e-8457-a58746b12866"),
                        Name = "Laptop",
                        Description = "High-performance laptop for professionals",
                        Price = 1299.99m,
                        Stock = 10,
                        Category = "Electronics",
                        ImageUrl = "laptop.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-30)
                    },
                    new Product
                    {
                        Id = Guid.Parse("a8c9badf-31e8-4c0b-a2c4-5d57d5f47a0c"),
                        Name = "Smartphone",
                        Description = "Latest smartphone with advanced features",
                        Price = 799.99m,
                        Stock = 15,
                        Category = "Electronics",
                        ImageUrl = "smartphone.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    new Product
                    {
                        Id = Guid.Parse("72ab34f7-e3c9-41d6-8e52-1b4cf57210c5"),
                        Name = "Headphones",
                        Description = "Noise-cancelling wireless headphones",
                        Price = 149.99m,
                        Stock = 20,
                        Category = "Audio",
                        ImageUrl = "headphones.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new Product
                    {
                        Id = Guid.Parse("f7c29693-1d99-4c75-b9c3-0c61e6b5d8c6"),
                        Name = "SAGA",
                        Description = "Test product for SAGA pattern",
                        Price = 99.99m,
                        Stock = 100,
                        Category = "Test",
                        ImageUrl = "test.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                foreach (var product in products)
                {
                    await _redisRepository.SetAsync(product.Id.ToString(), product);
                    _logger.LogInformation("Ürün Redis'e eklendi: {ProductId} - {ProductName}", product.Id, product.Name);
                }

                _logger.LogInformation("Toplam {Count} ürün başarıyla Redis'e eklendi", products.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Örnek ürünleri Redis'e eklerken hata oluştu");
                throw;
            }
        }
    }
}
