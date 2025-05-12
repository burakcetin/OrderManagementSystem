using MediatR;
using System;

namespace SharedKernel.Commands.Stock
{
    /// <summary>
    /// Ürün stoğunu geri eklemek için komut
    /// </summary>
    public class RevertStockCommand : IRequest<RevertStockResult>
    {
        /// <summary>
        /// Ürün ID
        /// </summary>
        public Guid ProductId { get; set; }
        
        /// <summary>
        /// Geri eklenecek miktar
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Stok geri ekleme işlemi sonucu
    /// </summary>
    public class RevertStockResult
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
        /// Geri eklenen miktar
        /// </summary>
        public int RestoredQuantity { get; set; }
        
        /// <summary>
        /// Yeni stok miktarı
        /// </summary>
        public int NewStockLevel { get; set; }
        
        /// <summary>
        /// Hata mesajı (başarısız ise)
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Başarılı işlem oluşturur
        /// </summary>
        public static RevertStockResult Success(Guid productId, int restoredQuantity, int newStockLevel)
        {
            return new RevertStockResult
            {
                IsSuccess = true,
                ProductId = productId,
                RestoredQuantity = restoredQuantity,
                NewStockLevel = newStockLevel,
                ErrorMessage = string.Empty
            };
        }

        /// <summary>
        /// Başarısız işlem oluşturur
        /// </summary>
        public static RevertStockResult Failure(Guid productId, string errorMessage)
        {
            return new RevertStockResult
            {
                IsSuccess = false,
                ProductId = productId,
                RestoredQuantity = 0,
                NewStockLevel = 0,
                ErrorMessage = errorMessage
            };
        }
    }
}
