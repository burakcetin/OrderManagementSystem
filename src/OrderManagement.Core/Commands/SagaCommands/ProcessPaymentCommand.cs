using MediatR;
using System;
using System.Collections.Generic;

namespace OrderManagement.Core.Commands.SagaCommands
{
    /// <summary>
    /// Ödeme işlemi yapmak için komut
    /// </summary>
    public class ProcessPaymentCommand : IRequest<ProcessPaymentResult>
    {
        /// <summary>
        /// Sipariş ID
        /// </summary>
        public Guid OrderId { get; set; }
        
        /// <summary>
        /// Ödeme tutarı
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Müşteri adı
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;
        
        /// <summary>
        /// Müşteri e-posta adresi
        /// </summary>
        public string CustomerEmail { get; set; } = string.Empty;
    }

    /// <summary>
    /// Ödeme işlemi sonucu
    /// </summary>
    public class ProcessPaymentResult
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Ödeme işlem ID (başarılı ise)
        /// </summary>
        public Guid? TransactionId { get; set; }
        
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
        public static ProcessPaymentResult Success(Guid transactionId)
        {
            return new ProcessPaymentResult
            {
                IsSuccess = true,
                TransactionId = transactionId,
                ErrorMessage = string.Empty
            };
        }

        /// <summary>
        /// Başarısız işlem oluşturur
        /// </summary>
        public static ProcessPaymentResult Failure(string errorMessage)
        {
            return new ProcessPaymentResult
            {
                IsSuccess = false,
                TransactionId = null,
                ErrorMessage = errorMessage
            };
        }
    }
}
