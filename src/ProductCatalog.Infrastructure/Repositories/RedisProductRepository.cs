using Microsoft.Extensions.Logging;
using ProductCatalog.Core.Entities;
using ProductCatalog.Core.Interfaces;
using SharedKernel.Data.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalog.Infrastructure.Repositories
{
    /// <summary>
    /// Redis kullanarak Product repository implementasyonu
    /// </summary>
    public class RedisProductRepository : IProductRepository
    {
        private readonly IRedisRepository<Product> _redisRepository;
        private readonly ILogger<RedisProductRepository> _logger;

        public RedisProductRepository(
            IRedisRepository<Product> redisRepository,
            ILogger<RedisProductRepository> logger)
        {
            _redisRepository = redisRepository;
            _logger = logger;
        }

        public async Task<Product> AddAsync(Product product)
        {
            _logger.LogInformation("Redis'e yeni ürün ekleniyor. ProductId: {ProductId}", product.Id);
            await _redisRepository.SetAsync(product.Id.ToString(), product);
            return product;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Redis'ten ürün siliniyor. ProductId: {ProductId}", id);
            return await _redisRepository.DeleteAsync(id.ToString());
        }

        public async Task<List<Product>> GetAllAsync()
        {
            _logger.LogInformation("Redis'ten tüm ürünler getiriliyor");
            var products = await _redisRepository.GetAllAsync();
            return products as List<Product> ?? new List<Product>(products);
        }

        public async Task<List<Product>> GetByCategoryAsync(string category)
        {
            _logger.LogInformation("Redis'ten kategoriye göre ürünler getiriliyor. Category: {Category}", category);
            
            // Kategoriye göre filtreleme yapmak için tüm ürünleri alıp filtreliyoruz
            // Not: Redis'in anahtar-değer yapısı nedeniyle bu yöntem kullanılabilir
            // Daha büyük uygulamalarda, kategori bilgisini anahtar yapısına dahil edip
            // desenle arama yapmak daha verimli olabilir
            var allProducts = await GetAllAsync();
            return allProducts
                .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && p.IsActive)
                .ToList();
        }

        public async Task<Product> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Redis'ten ürün getiriliyor. ProductId: {ProductId}", id);
            return await _redisRepository.GetByIdAsync(id.ToString());
        }

        public async Task UpdateAsync(Product product)
        {
            _logger.LogInformation("Redis'teki ürün güncelleniyor. ProductId: {ProductId}", product.Id);
            await _redisRepository.SetAsync(product.Id.ToString(), product);
        }
    }
}
