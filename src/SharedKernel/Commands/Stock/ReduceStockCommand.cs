using MediatR;
using System;

namespace SharedKernel.Commands.Stock
{
    /// <summary>
    /// Ürün stoğunu düşürmek için komut
    /// </summary>
    public class ReduceStockCommand : IRequest<ReduceStockResult>
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
    /// </summary>
    public class ReduceStockResult
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
        public static ReduceStockResult Success(Guid productId, int reducedQuantity, int newStockLevel)
        {
            return new ReduceStockResult
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
        public static ReduceStockResult Failure(Guid productId, string errorMessage)
        {
            return new ReduceStockResult
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
