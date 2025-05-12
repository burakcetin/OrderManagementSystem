using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCatalog.Core.Entities;
using ProductCatalog.Infrastructure.Data;
using SharedKernel.Data.Redis;
using System;
using System.Threading.Tasks;

namespace ProductCatalog.API.DataMigration
{
    /// <summary>
    /// InMemory veritabanından Redis'e veri geçişi için yardımcı sınıf
    /// </summary>
    public class MigrateProductsToRedis
    {
        private readonly ProductDbContext _dbContext;
        private readonly IRedisRepository<Product> _redisRepository;
        private readonly ILogger<MigrateProductsToRedis> _logger;

        public MigrateProductsToRedis(
            ProductDbContext dbContext,
            IRedisRepository<Product> redisRepository,
            ILogger<MigrateProductsToRedis> logger)
        {
            _dbContext = dbContext;
            _redisRepository = redisRepository;
            _logger = logger;
        }

        /// <summary>
        /// InMemory DB'deki tüm ürünleri Redis'e aktarır
        /// </summary>
        public async Task MigrateAllProductsAsync()
        {
            try
            {
                var products = await _dbContext.Products.ToListAsync();
                
                _logger.LogInformation("InMemory DB'den Redis'e {Count} ürün aktarılıyor", products.Count);
                
                foreach (var product in products)
                {
                    await _redisRepository.SetAsync(product.Id.ToString(), product);
                    _logger.LogInformation("Ürün Redis'e aktarıldı: {ProductId} - {ProductName}", product.Id, product.Name);
                }
                
                _logger.LogInformation("Toplam {Count} ürün başarıyla Redis'e aktarıldı", products.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürünleri Redis'e aktarırken hata oluştu");
                throw;
            }
        }
    }
}
