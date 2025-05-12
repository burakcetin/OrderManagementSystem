using MediatR;
using System;

namespace OrderManagement.Core.Commands.SagaCommands
{
    /// <summary>
    /// Ürün detaylarını sorgulama komutu
    /// </summary>
    public class GetProductDetailsQuery : IRequest<ProductDetailsResult>
    {
        /// <summary>
        /// Ürün ID
        /// </summary>
        public Guid ProductId { get; set; }
    }

    /// <summary>
    /// Ürün detayları sonucu
    /// </summary>
    public class ProductDetailsResult
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Ürün ID
        /// </summary>
        public Guid ProductId { get; set; }
        
        /// <summary>
        /// Ürün adı
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Ürün fiyatı
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Stok miktarı
        /// </summary>
        public int StockQuantity { get; set; }
        
        /// <summary>
        /// Ürün aktif mi?
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Hata mesajı (başarısız ise)
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Başarılı işlem oluşturur
        /// </summary>
        public static ProductDetailsResult Success(Guid productId, string name, decimal price, int stockQuantity, bool isActive)
        {
            return new ProductDetailsResult
            {
                IsSuccess = true,
                ProductId = productId,
                Name = name,
                Price = price,
                StockQuantity = stockQuantity,
                IsActive = isActive,
                ErrorMessage = string.Empty
            };
        }

        /// <summary>
        /// Başarısız işlem oluşturur
        /// </summary>
        public static ProductDetailsResult Failure(Guid productId, string errorMessage)
        {
            return new ProductDetailsResult
            {
                IsSuccess = false,
                ProductId = productId,
                ErrorMessage = errorMessage
            };
        }
    }
}
