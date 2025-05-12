using MediatR;
using System;

namespace OrderManagement.Core.Commands.SagaCommands
{
    /// <summary>
    /// Ödeme iadesi yapmak için komut
    /// </summary>
    public class RefundPaymentCommand : IRequest<RefundPaymentResult>
    {
        /// <summary>
        /// Ödeme işlem ID
        /// </summary>
        public Guid PaymentTransactionId { get; set; }
        
        /// <summary>
        /// İade edilecek miktar
        /// </summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Ödeme iadesi sonucu
    /// </summary>
    public class RefundPaymentResult
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Geri ödeme işlem ID (başarılı ise)
        /// </summary>
        public Guid? RefundTransactionId { get; set; }
        
        /// <summary>
        /// Hata mesajı (başarısız ise)
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// İşlem tarihi
        /// </summary>
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Başarılı işlem oluşturur
        /// </summary>
        public static RefundPaymentResult Success(Guid refundTransactionId)
        {
            return new RefundPaymentResult
            {
                IsSuccess = true,
                RefundTransactionId = refundTransactionId,
                ErrorMessage = string.Empty
            };
        }

        /// <summary>
        /// Başarısız işlem oluşturur
        /// </summary>
        public static RefundPaymentResult Failure(string errorMessage)
        {
            return new RefundPaymentResult
            {
                IsSuccess = false,
                RefundTransactionId = null,
                ErrorMessage = errorMessage
            };
        }
    }
}
