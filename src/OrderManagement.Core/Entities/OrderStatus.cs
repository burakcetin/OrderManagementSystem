namespace OrderManagement.Core.Entities
{
    public enum OrderStatus
    {
        Pending = 0,    // Sipariş başlatıldı (veritabanına kaydedildi, mesaj gönderildi)
        Created = 1,    // Worker tarafından başarıyla işleme alındı
        Processing = 2, // İşleniyor (SAGA başladı)
        Confirmed = 3,  // Onaylanmış
        Shipped = 4,    // Kargoya verilmiş
        Delivered = 5,  // Teslim edilmiş
        Cancelled = 6,  // İptal edilmiş
        Failed = 7,     // Başarısız olmuş
        Returned = 8    // İade edilmiş
    }
}