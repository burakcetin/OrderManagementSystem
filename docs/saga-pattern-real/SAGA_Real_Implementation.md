# Gerçekçi SAGA Pattern Uygulaması

Bu dokümanda, OrderManagementSystem projesi içinde uygulanan gerçekçi bir SAGA Pattern örneği açıklanmaktadır. Bu implementasyon, gerçek bir e-ticaret sistemindeki siparişlerin işlenmesi senaryosunu simüle etmektedir.

## SAGA Pattern Nedir?

SAGA Pattern, birden fazla servis veya işlem içeren dağıtık sistemlerde, işlemsel bütünlüğü (transactional integrity) sağlamak için kullanılan bir tasarım desenidir. Geleneksel tek veritabanı işlemlerinin ACID özelliklerini dağıtık sistemlerde uygulamaya çalışır.

SAGA Pattern'in iki ana yaklaşımı vardır:

1. **Koreografi (Choreography)**: Her servis bir olaya tepki olarak kendi işlemini yapar ve diğer servisleri tetikleyecek yeni olaylar yayınlar.
2. **Orkestrasyon (Orchestration)**: Merkezi bir koordinatör (orchestrator), tüm işlem adımlarını koordine eder, başarısızlık durumunda telafi işlemlerini (compensation) başlatır.

Bu gerçekçi uygulamada **Orkestrasyon** yaklaşımı kullanılmıştır.

## Projede Gerçekçi SAGA Pattern Uygulaması

OrderManagementSystem projesinde, sipariş oluşturma işlemi artık SAGA Pattern kullanılarak gerçekleştirilmektedir. Bu akış şu adımları içerir:

1. **Ödeme İşlemi**: Müşteriden ödeme alınması
2. **Stok Düşme**: Ürün stokundan sipariş edilen miktarın düşülmesi
3. **Sipariş Durum Güncelleme**: Sipariş durumunun güncellenmesi

Her adım başarılı olursa işlem ilerler. Herhangi bir adımda başarısızlık olursa, tamamlanmış tüm adımlar telafi işlemleri ile geri alınır.

## Teknik Detaylar

### Entity ve Servis Değişiklikleri

1. **Order Entity'sine ProductId eklendi**:
   ```csharp
   public class Order
   {
       public Guid Id { get; set; }
       public Guid ProductId { get; set; } // Yeni eklenen alan
       // Diğer özellikler...
   }
   ```

2. **IProductService ve IPaymentService arayüzleri eklendi**:
   ```csharp
   public interface IProductService
   {
       Task<bool> ReduceStockAsync(Guid productId, int quantity);
       Task<bool> RevertStockReductionAsync(Guid productId, int quantity);
       Task<bool> CheckStockAvailabilityAsync(Guid productId, int quantity);
       Task<ProductInfo> GetProductInfoAsync(Guid productId);
   }

   public interface IPaymentService
   {
       Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, CustomerInfo customerInfo);
       Task<bool> RefundPaymentAsync(Guid paymentTransactionId, decimal amount);
   }
   ```

3. **ProductService ve PaymentService implementasyonları oluşturuldu**:
   - `ProductService`: Ürün stok işlemlerini gerçekleştirir
   - `PaymentService`: Ödeme işlemlerini simüle eder

### SAGA Orkestratörü

OrderSagaOrchestrator, işlemin koordinasyonundan sorumludur:

```csharp
public class OrderSagaOrchestrator : IOrderSagaOrchestrator
{
    // Bağımlılıklar...

    public async Task<SagaResult<OrderSagaData>> ProcessOrderAsync(Order order)
    {
        // SAGA adımlarını tanımla ve gerçekleştir
        // Başarısızlık durumunda telafi işlemlerini koordine et
    }
}
```

### Veri Taşıma ve Takip Modelleri

```csharp
public class OrderSagaData
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public Order OrderDetails { get; set; }
    public Guid? PaymentTransactionId { get; set; }
    public bool StockReduced { get; set; }
    public int ReducedStockQuantity { get; set; }
    public List<string> StepLogs { get; set; } = new List<string>();
    
    // Log ekleme yardımcı metodu
    public void AddStepLog(string log) { ... }
}
```

## Akış Şeması

```
┌────────────────────┐
│ Sipariş Oluşturma  │
└─────────┬──────────┘
          │
┌─────────▼──────────┐
│ Ürün Kontrolü      │
└─────────┬──────────┘
          │
┌─────────▼──────────┐
│ SAGA Başlatılması  │
└─────────┬──────────┘
          │
┌─────────▼──────────┐
│ Adım 1: Ödeme      ├────────┐
└─────────┬──────────┘        │
          │ Başarılı          │ Başarısız
┌─────────▼──────────┐        │
│ Adım 2: Stok Düşme ├────┐   │
└─────────┬──────────┘    │   │
          │ Başarılı      │   │
┌─────────▼──────────┐    │   │
│ Adım 3: Sipariş    │    │   │
│ Güncelleme         ├─┐  │   │
└─────────┬──────────┘ │  │   │
          │ Başarılı   │  │   │
┌─────────▼──────────┐ │  │   │
│ SAGA Başarılı      │ │  │   │
│ Sipariş: Shipped   │ │  │   │
└────────────────────┘ │  │   │
                        │  │   │
┌──────────────────────┐  │   │
│ Telafi İşlemi:       │◄─┘   │
│ Siparişi Failed      │      │
│ Olarak İşaretle      │      │
└──────────┬───────────┘      │
           │                   │
┌──────────▼───────────┐      │
│ Telafi İşlemi:       │◄─────┘
│ Stok Geri Ekleme     │
│ Ödeme İadesi         │
└──────────────────────┘
```

## Test Senaryoları

Projede iki test senaryosu mevcuttur:

1. **Başarılı Sipariş**: Ödeme, stok düşme ve sipariş güncelleme başarılıdır.
2. **Stok Sorunu**: Ödeme başarılı, ancak stok düşme işlemi yetersiz stok nedeniyle başarısız olabilir. Bu durumda ödeme iadesi telafi işlemi çalışır.

## Gerçek Web API Test Endpointi

```http
POST /api/orders/create-test-order
```

Bu endpoint, gerçek bir ürünle sipariş oluşturma işlemini test etmek için kullanılır:
- Ürün: Headphones (ID: 72ab34f7-e3c9-41d6-8e52-1b4cf57210c5)
- Miktar: 2 adet (20 adet stok mevcut)

## Örnek Senaryolar

### Başarılı Senaryo

1. Sipariş oluşturma talep edilir
2. Ödeme başarıyla gerçekleştirilir
3. Ürün stokundan 2 adet düşülür
4. Sipariş durumu "Processing" olarak güncellenir
5. SAGA işlemi başarıyla tamamlanır
6. Sipariş durumu "Shipped" olarak güncellenir

### Başarısız Senaryo (Stok Yetersizse)

1. Sipariş oluşturma talep edilir
2. Ödeme başarıyla gerçekleştirilir
3. Ürün stokundan düşülme yapılmaya çalışılır ancak stok yetersiz (0 adet varsa)
4. SAGA telafi işlemi başlar
5. Ödeme iadesi gerçekleştirilir
6. Sipariş durumu "Failed" olarak işaretlenir

### Başarısız Senaryo (Ödeme Başarısızsa)

1. Sipariş oluşturma talep edilir
2. Ödeme işlemi başarısız olur (yüksek tutarlı sipariş olduğunda %20 olasılıkla)
3. SAGA işlemi hemen sona erer, telafi işlemi gerekmez
4. Sipariş durumu "Failed" olarak işaretlenir

## Teknik Notlar

1. **Rastgele Başarısızlık Simülasyonu**:
   - Ödeme servisi yüksek tutarlı ödemelerde (%20 olasılıkla başarısız olur)
   - Ödeme iadesi %5 olasılıkla başarısız olabilir (manuel müdahale senaryosu)

2. **Telafi İşlemleri**:
   - Tamamlanmış adımlar ters sırayla telafi edilir
   - Telafi işlemi başarısız olursa log kaydedilir (gerçek senaryoda alarm sistemi olabilir)

3. **Durum İzleme**:
   - Her adımda detaylı loglar tutulur
   - SAGA veri modelinde tüm adımların sonuçları kaydedilir

## Kullanım ve Geliştirme Önerileri

1. **Daha Fazla SAGA Adımı Eklemek**:
   - SagaContext'e yeni adımlar eklenebilir
   - Her adım için ExecuteAction ve CompensateAction belirtilmelidir

2. **Daha Fazla Servis Entegrasyonu**:
   - Kargo servisi, bildirim servisi gibi yeni servisler eklenebilir
   - OrderSagaOrchestrator'a yeni bağımlılıklar enjekte edilmelidir

3. **Kalıcı SAGA Durumu**:
   - Gerçek bir uygulamada SAGA durumu veritabanında saklanmalıdır
   - İşlem ilerledikçe durum güncellenmelidir

4. **Daha İyi Hata Yönetimi**:
   - Telafi işlemleri için yeniden deneme mekanizması
   - Hata kategorileri (geçici vs. kalıcı) ve farklı stratejiler

## Sonuç

Bu gerçekçi SAGA Pattern uygulaması, dağıtık işlemlerde tutarlılığı sağlamanın etkili bir yolunu göstermektedir. Projede, ürün stoku ve ödeme servisleri gibi gerçek dünya entegrasyonları simüle edilmiştir. Bu sayede, mikro servis mimarilerinde yaygın olarak kullanılan SAGA pattern'in pratikte nasıl uygulanabileceği gösterilmiştir.
