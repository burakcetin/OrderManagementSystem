using OrderManagement.Core.Entities;
using System.Threading.Tasks;

namespace OrderManagement.Core.Services
{
    /// <summary>
    /// Email gönderme servisi arayüzü
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sipariş onay emaili gönderir
        /// </summary>
        Task SendOrderConfirmationAsync(Order order);
    }
}
