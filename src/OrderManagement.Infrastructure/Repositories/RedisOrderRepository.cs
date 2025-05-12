using Microsoft.Extensions.Logging;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using SharedKernel.Data.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Redis kullanarak Order repository implementasyonu
    /// </summary>
    public class RedisOrderRepository : IOrderRepository
    {
        private readonly IRedisRepository<Order> _redisRepository;
        private readonly ILogger<RedisOrderRepository> _logger;

        public RedisOrderRepository(
            IRedisRepository<Order> redisRepository,
            ILogger<RedisOrderRepository> logger)
        {
            _redisRepository = redisRepository;
            _logger = logger;
        }

        public async Task<Order> AddAsync(Order order)
        {
            _logger.LogInformation("Redis'e yeni sipariş ekleniyor. OrderId: {OrderId}", order.Id);
            await _redisRepository.SetAsync(order.Id.ToString(), order);
            return order;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Redis'ten sipariş siliniyor. OrderId: {OrderId}", id);
            return await _redisRepository.DeleteAsync(id.ToString());
        }

        public async Task<List<Order>> GetAllAsync()
        {
            _logger.LogInformation("Redis'ten tüm siparişler getiriliyor");
            var orders = await _redisRepository.GetAllAsync();
            return orders as List<Order> ?? new List<Order>(orders);
        }

        public async Task<Order> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Redis'ten sipariş getiriliyor. OrderId: {OrderId}", id);
            return await _redisRepository.GetByIdAsync(id.ToString());
        }

        public async Task UpdateAsync(Order order)
        {
            _logger.LogInformation("Redis'teki sipariş güncelleniyor. OrderId: {OrderId}", order.Id);
            await _redisRepository.SetAsync(order.Id.ToString(), order);
        }
    }
}
