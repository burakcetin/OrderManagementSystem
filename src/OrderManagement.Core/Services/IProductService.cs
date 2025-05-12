using System;
using System.Threading.Tasks;

namespace OrderManagement.Core.Services
{
    /// <summary>
    /// Ürün servisinin arayüzü
    /// </summary>
    public class ProductStockResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int RemainingStock { get; set; }
    }

    public interface IProductService
    {
        /// <summary>
        /// Ürün stoğunu azaltır
        /// </summary>
        Task<ProductStockResult> ReduceStockAsync(Guid productId, int quantity);

        /// <summary>
        /// Ürün stoğunu geri yükler
        /// </summary>
        Task<ProductStockResult> RestoreStockAsync(Guid productId, int quantity);
    }
}
