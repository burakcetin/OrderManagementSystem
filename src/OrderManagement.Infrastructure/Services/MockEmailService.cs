using Microsoft.Extensions.Logging;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Services;
using System;
using System.Threading.Tasks;

namespace OrderManagement.Infrastructure.Services
{
    /// <summary>
    /// Mock email gönderme servisi
    /// </summary>
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sipariş onay emaili gönderir (mock)
        /// </summary>
        public async Task SendOrderConfirmationAsync(Order order)
        {
            _logger.LogInformation("Email gönderiliyor to {Email}. OrderId: {OrderId}", order.CustomerEmail, order.Id);
            
            // Email gönderme işlemini simüle etmek için
            await Task.Delay(300);
            
            // Bazen hata fırlatmak için küçük bir olasılık
            if (new Random().Next(0, 20) == 0) // 5% şans
            {
                throw new Exception("Email gönderilemedi (simüle edilmiş hata)");
            }
            
            _logger.LogInformation("Email başarıyla gönderildi to {Email}. OrderId: {OrderId}", 
                order.CustomerEmail, order.Id);
        }
    }
}
