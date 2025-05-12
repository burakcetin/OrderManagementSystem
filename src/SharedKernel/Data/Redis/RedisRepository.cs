using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SharedKernel.Data.Redis
{
    /// <summary>
    /// Redis veritabanı için repository uygulaması
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    public class RedisRepository<T> : IRedisRepository<T> where T : class
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisRepository<T>> _logger;
        private readonly string _prefix;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public RedisRepository(IConnectionMultiplexer redis, ILogger<RedisRepository<T>> logger, string prefix = "")
        {
            _database = redis.GetDatabase();
            _logger = logger;
            _prefix = string.IsNullOrEmpty(prefix) ? typeof(T).Name + ":" : prefix + ":";
        }

        /// <summary>
        /// Anahtarı formatlar
        /// </summary>
        private string FormatKey(string id) => $"{_prefix}{id}";

        /// <summary>
        /// Bir varlığı ID'ye göre getirir
        /// </summary>
        public async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                var key = FormatKey(id);
                var data = await _database.StringGetAsync(key);

                if (data.IsNullOrEmpty)
                {
                    return null;
                }

                return JsonSerializer.Deserialize<T>(data!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis'ten veriyi getirirken hata oluştu. Key: {Key}", id);
                throw;
            }
        }

        /// <summary>
        /// Belirtilen desene uyan tüm anahtarları getirir
        /// </summary>
        public async Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern)
        {
            var keys = new List<string>();
            var formattedPattern = FormatKey(pattern);

            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var redisKeys = server.Keys(pattern: formattedPattern);

                foreach (var redisKey in redisKeys)
                {
                    // Prefix'i kaldırarak orijinal ID'yi alıyoruz
                    var key = redisKey.ToString().Substring(_prefix.Length);
                    keys.Add(key);
                }

                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis desenine göre anahtarları alırken hata oluştu. Pattern: {Pattern}", pattern);
                throw;
            }
        }

        /// <summary>
        /// Tüm varlıkları getirir
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var result = new List<T>();
            
            try
            {
                var pattern = FormatKey("*");
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern).ToArray();

                if (keys.Length == 0)
                {
                    return result;
                }

                var values = await _database.StringGetAsync(keys);
                
                foreach (var value in values)
                {
                    if (!value.IsNullOrEmpty)
                    {
                        var item = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
                        if (item != null)
                        {
                            result.Add(item);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis'ten tüm verileri getirirken hata oluştu");
                throw;
            }
        }

        /// <summary>
        /// Varlık ekler veya günceller
        /// </summary>
        public async Task SetAsync(string id, T entity, TimeSpan? expiry = null)
        {
            try
            {
                var key = FormatKey(id);
                var json = JsonSerializer.Serialize(entity, _jsonOptions);
                
                if (expiry.HasValue)
                {
                    await _database.StringSetAsync(key, json, expiry);
                }
                else
                {
                    await _database.StringSetAsync(key, json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis'e veri kaydederken hata oluştu. Key: {Key}", id);
                throw;
            }
        }

        /// <summary>
        /// Bir varlığı siler
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var key = FormatKey(id);
                return await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis'ten veri silerken hata oluştu. Key: {Key}", id);
                throw;
            }
        }

        /// <summary>
        /// Belirtilen desene uyan tüm anahtarları siler
        /// </summary>
        public async Task<long> DeleteByPatternAsync(string pattern)
        {
            try
            {
                var formattedPattern = FormatKey(pattern);
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: formattedPattern).ToArray();

                if (keys.Length == 0)
                {
                    return 0;
                }

                return await _database.KeyDeleteAsync(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis'ten desene göre veri silerken hata oluştu. Pattern: {Pattern}", pattern);
                throw;
            }
        }

        /// <summary>
        /// Bir anahtarın var olup olmadığını kontrol eder
        /// </summary>
        public async Task<bool> ExistsAsync(string id)
        {
            try
            {
                var key = FormatKey(id);
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis'te anahtar kontrolü sırasında hata oluştu. Key: {Key}", id);
                throw;
            }
        }
    }
}
