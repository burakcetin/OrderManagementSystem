using System;

namespace OrderManagement.Core.Messages
{
    /// <summary>
    /// Sipariş oluşturulduğunda Rebus ile yayınlanacak mesaj
    /// </summary>
    public class OrderCreatedMessage
    {
        /// <summary>
        /// Sipariş ID
        /// </summary>
        public Guid OrderId { get; set; }
        
        /// <summary>
        /// Ürün Adı
        /// </summary>
        public string ProductName { get; set; }
        
        /// <summary>
        /// Fiyat
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Ürün Miktarı
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Müşteri Adı
        /// </summary>
        public string CustomerName { get; set; }
        
        /// <summary>
        /// Müşteri Email
        /// </summary>
        public string CustomerEmail { get; set; }
        
        /// <summary>
        /// Teslimat Adresi
        /// </summary>
        public string ShippingAddress { get; set; }
        
        /// <summary>
        /// Ürün ID - Opsiyonel
        /// </summary>
        public Guid? ProductId { get; set; }
        
        /// <summary>
        /// Oluşturulma Zamanı - Opsiyonel
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }
}
