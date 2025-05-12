using System;
using System.Threading.Tasks;

namespace OrderManagement.Core.Services
{
    /// <summary>
    /// Ödeme servisi arayüzü
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Ödeme işlemi gerçekleştirir
        /// </summary>
        /// <param name="orderId">Sipariş ID</param>
        /// <param name="amount">Ödeme miktarı</param>
        /// <param name="customerInfo">Müşteri bilgileri</param>
        /// <returns>Ödeme sonucu</returns>
        Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, CustomerInfo customerInfo);
        
        /// <summary>
        /// Ödeme iadesi yapar
        /// </summary>
        /// <param name="paymentTransactionId">Ödeme işlem ID</param>
        /// <param name="amount">İade edilecek miktar</param>
        /// <returns>İade başarılı mı?</returns>
        Task<bool> RefundPaymentAsync(Guid paymentTransactionId, decimal amount);
    }
    
    /// <summary>
    /// Ödeme sonucu
    /// </summary>
    public class PaymentResult
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Ödeme işlem ID (başarılı ise dolu olur)
        /// </summary>
        public Guid? TransactionId { get; set; }
        
        /// <summary>
        /// Hata mesajı (başarısız ise dolu olur)
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// İşlem tarihi
        /// </summary>
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Müşteri bilgileri
    /// </summary>
    public class CustomerInfo
    {
        /// <summary>
        /// Müşteri adı
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Müşteri e-posta adresi
        /// </summary>
        public string Email { get; set; }
    }
}
