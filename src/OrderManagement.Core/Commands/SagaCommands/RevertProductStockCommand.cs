using MediatR;
using System;

// NOT: Bu komut artık kullanılmayacak
// SharedKernel.Commands.Stock.RevertStockCommand kullanılacak.
// Geriye dönük uyumluluk için tutulmaktadır.

namespace OrderManagement.Core.Commands.SagaCommands
{
    /// <summary>
    /// Ürün stoğunu geri eklemek için komut
    /// DEPRECATED: Yerine SharedKernel.Commands.Stock.RevertStockCommand kullanın
    /// </summary>
    [Obsolete("Yerine SharedKernel.Commands.Stock.RevertStockCommand kullanın")]
    public class RevertProductStockCommand : IRequest<RevertProductStockResult>
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
    /// DEPRECATED: Yerine SharedKernel.Commands.Stock.RevertStockResult kullanın
    /// </summary>
    [Obsolete("Yerine SharedKernel.Commands.Stock.RevertStockResult kullanın")]
    public class RevertProductStockResult
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
        public static RevertProductStockResult Success(Guid productId, int restoredQuantity, int newStockLevel)
        {
            return new RevertProductStockResult
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
        public static RevertProductStockResult Failure(Guid productId, string errorMessage)
        {
            return new RevertProductStockResult
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
