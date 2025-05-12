using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedKernel.Data.Redis
{
    /// <summary>
    /// Redis veritabanı için repository arayüzü
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    public interface IRedisRepository<T> where T : class
    {
        /// <summary>
        /// Bir varlığı ID'ye göre getirir
        /// </summary>
        Task<T?> GetByIdAsync(string id);
        
        /// <summary>
        /// Belirtilen desene uyan tüm anahtarları getirir
        /// </summary>
        Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern);
        
        /// <summary>
        /// Tüm varlıkları getirir
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Varlık ekler veya günceller
        /// </summary>
        Task SetAsync(string id, T entity, TimeSpan? expiry = null);
        
        /// <summary>
        /// Bir varlığı siler
        /// </summary>
        Task<bool> DeleteAsync(string id);
        
        /// <summary>
        /// Belirtilen desene uyan tüm anahtarları siler
        /// </summary>
        Task<long> DeleteByPatternAsync(string pattern);
        
        /// <summary>
        /// Bir anahtarın var olup olmadığını kontrol eder
        /// </summary>
        Task<bool> ExistsAsync(string id);
    }
}
