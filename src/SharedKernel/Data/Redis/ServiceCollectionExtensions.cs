using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace SharedKernel.Data.Redis
{
    /// <summary>
    /// Redis için servis kayıt uzantıları
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Redis servislerini kaydeder
        /// </summary>
        public static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Redis bağlantısını yapılandır
            var redisConnectionString = configuration.GetConnectionString("RedisConnection");
            
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new InvalidOperationException("Redis bağlantı dizgisi yapılandırmada bulunamadı.");
            }
            
            // Redis ConnectionMultiplexer'ı singleton olarak kaydediyoruz
            services.AddSingleton<IConnectionMultiplexer>(sp => 
                ConnectionMultiplexer.Connect(redisConnectionString));
            
            return services;
        }
        
        /// <summary>
        /// Belirtilen entity için Redis repository'sini kaydeder
        /// </summary>
        public static IServiceCollection AddRedisRepository<T>(this IServiceCollection services, string? prefix = null) where T : class
        {
            services.AddScoped<IRedisRepository<T>>(sp =>
            {
                var redis = sp.GetRequiredService<IConnectionMultiplexer>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RedisRepository<T>>>();
                return new RedisRepository<T>(redis, logger, prefix ?? typeof(T).Name);
            });
            
            return services;
        }
    }
}
