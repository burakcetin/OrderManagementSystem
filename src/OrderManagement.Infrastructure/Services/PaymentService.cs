using Microsoft.Extensions.Logging;
using OrderManagement.Core.Services;
using System;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure.Services
{
    /// <summary>
    /// Ödeme servisi implementasyonu (simülasyon)
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        private Random _random = new Random();

        /// <summary>
        /// Constructor
        /// </summary>
        public PaymentService(ILogger<PaymentService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Ödeme işlemi gerçekleştirir (simülasyon)
        /// </summary>
        public async Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, CustomerInfo customerInfo)
        {
            try
            {
                _logger.LogInformation("Ödeme işlemi başlatılıyor. OrderId: {OrderId}, Amount: {Amount}, Customer: {CustomerName}",
                    orderId, amount, customerInfo.Name);
                
                // Simülasyon için bekleme
                await Task.Delay(500);

                // Gerçek bir ödeme entegrasyonu yerine simülasyon
                // Yüksek tutarlı ödemelerde %20 olasılıkla hata döner (test için)
                if (amount > 1000 && _random.Next(100) < 20)
                {
                    _logger.LogWarning("Ödeme reddedildi: Yüksek tutar. OrderId: {OrderId}, Amount: {Amount}",
                        orderId, amount);
                    
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        TransactionId = null,
                        ErrorMessage = "Ödeme reddedildi: Yüksek tutarlı işlem onaylanmadı."
                    };
                }
                
                // Başarılı ödeme simülasyonu
                var transactionId = Guid.NewGuid();
                
                _logger.LogInformation("Ödeme başarıyla tamamlandı. OrderId: {OrderId}, TransactionId: {TransactionId}",
                    orderId, transactionId);
                
                return new PaymentResult
                {
                    IsSuccess = true,
                    TransactionId = transactionId,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme işleminde hata. OrderId: {OrderId}, Amount: {Amount}", 
                    orderId, amount);
                
                return new PaymentResult
                {
                    IsSuccess = false,
                    TransactionId = null,
                    ErrorMessage = $"Ödeme işleminde hata: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Ödeme iadesi yapar (simülasyon)
        /// </summary>
        public async Task<bool> RefundPaymentAsync(Guid paymentTransactionId, decimal amount)
        {
            try
            {
                _logger.LogInformation("Ödeme iadesi başlatılıyor. TransactionId: {TransactionId}, Amount: {Amount}",
                    paymentTransactionId, amount);
                
                // Simülasyon için bekleme
                await Task.Delay(300);

                // %5 olasılıkla iade hatası (test için)
                if (_random.Next(100) < 5)
                {
                    _logger.LogWarning("Ödeme iadesi başarısız oldu. TransactionId: {TransactionId}, Amount: {Amount}",
                        paymentTransactionId, amount);
                    return false;
                }
                
                _logger.LogInformation("Ödeme iadesi başarıyla tamamlandı. TransactionId: {TransactionId}, Amount: {Amount}",
                    paymentTransactionId, amount);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödeme iadesi işleminde hata. TransactionId: {TransactionId}, Amount: {Amount}",
                    paymentTransactionId, amount);
                return false;
            }
        }
    }
}
