using OrderManagement.Core.Entities;
using System;
using System.Collections.Generic;

namespace OrderManagement.Core.Sagas.OrderSaga
{
    /// <summary>
    /// Sipariş SAGA'sı için veri modeli
    /// </summary>
    public class OrderSagaData
    {
        /// <summary>
        /// Sipariş kimliği
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Ürün kimliği
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Sipariş bilgileri
        /// </summary>
        public Order OrderDetails { get; set; }

        /// <summary>
        /// Ödeme işlem kimliği
        /// </summary>
        public Guid? PaymentTransactionId { get; set; }

        /// <summary>
        /// Stok rezervasyonu yapıldı mı?
        /// </summary>
        public bool StockReduced { get; set; }

        /// <summary>
        /// Düşülen stok miktarı
        /// </summary>
        public int ReducedStockQuantity { get; set; }

        /// <summary>
        /// Gerçekleşen adımların audit log'u
        /// </summary>
        public List<string> StepLogs { get; set; } = new List<string>();

        /// <summary>
        /// Adım logunu ekler
        /// </summary>
        /// <param name="log">Log mesajı</param>
        public void AddStepLog(string log)
        {
            StepLogs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {log}");
        }
    }
}
