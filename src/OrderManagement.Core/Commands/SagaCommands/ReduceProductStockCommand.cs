using MediatR;
using System;

// NOT: Bu komut artık kullanılmayacak
// SharedKernel.Commands.Stock.ReduceStockCommand kullanılacak.
// Geriye dönük uyumluluk için tutulmaktadır.

namespace OrderManagement.Core.Commands.SagaCommands
{
    /// <summary>
    /// Ürün stoğunu düşürmek için komut
    /// DEPRECATED: Yerine SharedKernel.Commands.Stock.ReduceStockCommand kullanın
    /// </summary>
    [Obsolete("Yerine SharedKernel.Commands.Stock.ReduceStockCommand kullanın")]
    public class ReduceProductStockCommand : IRequest<ReduceProductStockResult>
    {
        /// <summary>
        /// Ürün ID
        /// </summary>
        public Guid ProductId { get; set; }
        
        /// <summary>
        /// Düşülecek miktar
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Stok düşme işlemi sonucu
    /// DEPRECATED: Yerine SharedKernel.Commands.Stock.ReduceStockResult kullanın
    /// </summary>
    [Obsolete("Yerine SharedKernel.Commands.Stock.ReduceStockResult kullanın")]
    public class ReduceProductStockResult
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
        /// Düşülen miktar
        /// </summary>
        public int ReducedQuantity { get; set; }
        
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
        public static ReduceProductStockResult Success(Guid productId, int reducedQuantity, int newStockLevel)
        {
            return new ReduceProductStockResult
            {
                IsSuccess = true,
                ProductId = productId,
                ReducedQuantity = reducedQuantity,
                NewStockLevel = newStockLevel,
                ErrorMessage = string.Empty
            };
        }

        /// <summary>
        /// Başarısız işlem oluşturur
        /// </summary>
        public static ReduceProductStockResult Failure(Guid productId, string errorMessage)
        {
            return new ReduceProductStockResult
            {
                IsSuccess = false,
                ProductId = productId,
                ReducedQuantity = 0,
                NewStockLevel = 0,
                ErrorMessage = errorMessage
            };
        }
    }
}
