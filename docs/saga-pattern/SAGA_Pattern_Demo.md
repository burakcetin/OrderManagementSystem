# SAGA Pattern Demonstrasyonu

Bu dokümanda OrderManagementSystem projesi içinde uygulanan SAGA Pattern örneği açıklanmaktadır.

## SAGA Pattern Nedir?

SAGA Pattern, birden fazla servis veya işlem içeren dağıtık sistemlerde, işlemsel bütünlüğü (transactional integrity) sağlamak için kullanılan bir tasarım desenidir. ACID özellikleri sunan geleneksel tek veritabanı işlemleri yerine, birden fazla servis arasında tutarlılık sağlar.

SAGA Pattern'in iki ana yaklaşımı vardır:

1. **Koreografi (Choreography)**: Her servis bir olaya tepki olarak kendi işlemini yapar ve diğer servisleri tetikleyecek yeni olaylar yayınlar.
2. **Orkestrasyon (Orchestration)**: Merkezi bir koordinatör (orchestrator), tüm işlem adımlarını koordine eder, başarısızlık durumunda telafi işlemlerini (compensation) başlatır.

Bu projede **Orkestrasyon** yaklaşımı uygulanmıştır.

## Projede SAGA Pattern Uygulaması

OrderManagementSystem projesinde, `ProductName` olarak "SAGA" değeri içeren siparişler için özel bir SAGA akışı uygulanmıştır. Bu akış, dağıtık bir e-ticaret sistemindeki sipariş sürecini simüle eder:

1. **Ödeme İşlemi**: Müşteriden ödeme alınması
2. **Stok Rezervasyonu**: Ürün stokunun rezerve edilmesi
3. **Sevkiyat Oluşturma**: Kargo/nakliye kaydının oluşturulması
4. **Sipariş Güncelleme**: Sipariş durumunun güncellenmesi

Her adım başarılı olursa sipariş süreci ilerler. Herhangi bir adımda başarısızlık olursa, tamamlanmış tüm adımlar telafi işlemleri ile geri alınır.

## Proje Bileşenleri

### 1. SAGA Veri Modelleri

```
└── OrderManagement.Core/Sagas/
    ├── ISagaCoordinator.cs          # SAGA koordinatör arayüzü
    ├── SagaContext.cs               # SAGA bağlam sınıfı
    ├── SagaStep.cs                  # SAGA adım modeli
    ├── SagaResult.cs                # SAGA işlem sonucu
    └── OrderSaga/
        ├── OrderSagaData.cs         # Sipariş SAGA veri modeli
        └── IOrderSagaOrchestrator.cs # Sipariş SAGA orkestratör arayüzü
```

### 2. SAGA Uygulaması

```
└── OrderManagement.Infrastructure/Sagas/
    ├── SagaCoordinator.cs           # SAGA koordinatör implementasyonu
    └── OrderSaga/
        └── OrderSagaOrchestrator.cs # Sipariş SAGA orkestratörü
```

### 3. Controller ve Handler Entegrasyonu

CreateOrderCommand handler'ında, "SAGA" adlı ürünler için SAGA işleminin başlatılması:

```csharp
// ProductName "SAGA" ise SAGA Pattern uygulanır
if (createdOrder.ProductName == "SAGA")
{
    // SAGA işlemini başlat
    var sagaResult = await _sagaOrchestrator.ProcessOrderAsync(createdOrder);
    
    // SAGA sonucu işleme...
}
```

Ayrıca test amaçlı SAGA endpoint'i eklendi:

```csharp
[HttpPost("saga-test")]
public async Task<ActionResult<ApiResponse<OrderDto>>> CreateSagaTest()
{
    // SAGA Test siparişi oluştur...
}
```

## Akış Şeması

```
1. Sipariş oluşturma (SAGA ürün adı ile)
    │
    ▼
2. SAGA Orchestrator başlatılması
    │
    ▼
3. Ödeme İşlemi ────────┐
    │                    │
    │ Başarılı           │ Başarısız
    ▼                    │
4. Stok Rezervasyonu     │
    │                    │
    │ Başarılı           │
    ▼                    │
5. Sevkiyat Oluşturma    │
    │                    │
    │ Başarılı           │
    ▼                    │
6. Sipariş Güncelleme    │
    │                    │
    │ Başarılı           │
    ▼                    ▼
7a. İşlem Başarılı   7b. Telafi İşlemleri
    │                    │
    ▼                    ▼
8a. Shipped durumu   8b. Failed durumu
```

## Telafi (Compensation) İşlemleri

Başarısızlık durumunda, başarılı olan adımlar için telafi işlemleri ters sırayla uygulanır:

1. Ödeme İptal/İade
2. Stok Rezervasyonu İptali
3. Sevkiyat İptali
4. Sipariş Durumu Failed olarak güncelleme

## Test Senaryoları

Her adım, test amaçlı olarak rastgele başarısızlık gösterebilecek şekilde yapılandırılmıştır:

- Ödeme İşlemi: 40% başarısızlık olasılığı
- Stok Rezervasyonu: 30% başarısızlık olasılığı
- Sevkiyat Oluşturma: 20% başarısızlık olasılığı
- Sipariş Güncelleme: 10% başarısızlık olasılığı

Bu rastgele başarısızlıklar, SAGA pattern'in telafi mekanizmasını test etmek için kullanılmıştır.

## API Üzerinden Test

```http
POST /api/orders/saga-test
```

Bu endpoint, "SAGA" olarak adlandırılmış özel bir ürünle sipariş oluşturur ve SAGA akışını başlatır. SAGA işlemi sonucuna bağlı olarak, sipariş "Shipped" (başarılı) veya "Failed" (başarısız) durumuna geçer.

## Log Örneği (Başarılı Senaryo)

```
[INF] SAGA başlatılıyor. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] SAGA adımı çalıştırılıyor: Ödeme işlemi. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] Ödeme işlemi simüle ediliyor. OrderId: 5fa85f64-5717-4562-b3fc-2c963f66afb7
[INF] SAGA adımı başarıyla tamamlandı: Ödeme işlemi. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] SAGA adımı çalıştırılıyor: Stok rezervasyonu. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] Stok rezervasyonu simüle ediliyor. OrderId: 5fa85f64-5717-4562-b3fc-2c963f66afb7
[INF] SAGA adımı başarıyla tamamlandı: Stok rezervasyonu. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] SAGA adımı çalıştırılıyor: Sevkiyat oluşturma. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] Sevkiyat oluşturma simüle ediliyor. OrderId: 5fa85f64-5717-4562-b3fc-2c963f66afb7
[INF] SAGA adımı başarıyla tamamlandı: Sevkiyat oluşturma. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] SAGA adımı çalıştırılıyor: Sipariş güncelleme. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] Sipariş durumu güncelleniyor. OrderId: 5fa85f64-5717-4562-b3fc-2c963f66afb7
[INF] SAGA adımı başarıyla tamamlandı: Sipariş güncelleme. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] SAGA başarıyla tamamlandı. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

## Log Örneği (Başarısız Senaryo)

```
[INF] SAGA başlatılıyor. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] SAGA adımı çalıştırılıyor: Ödeme işlemi. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] Ödeme işlemi simüle ediliyor. OrderId: 5fa85f64-5717-4562-b3fc-2c963f66afb7
[INF] SAGA adımı başarıyla tamamlandı: Ödeme işlemi. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] SAGA adımı çalıştırılıyor: Stok rezervasyonu. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] Stok rezervasyonu simüle ediliyor. OrderId: 5fa85f64-5717-4562-b3fc-2c963f66afb7
[WRN] Stok rezervasyonu test amaçlı başarısız oldu. OrderId: 5fa85f64-5717-4562-b3fc-2c963f66afb7
[WRN] SAGA adımı başarısız oldu: Stok rezervasyonu. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6. Telafi işlemleri başlatılıyor.
[INF] Telafi adımı çalıştırılıyor: Ödeme işlemi. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] Ödeme iadesi yapılıyor. İşlem ID: 6fa85f64-5717-4562-b3fc-2c963f66afc8, OrderId: 5fa85f64-5717-4562-b3fc-2c963f66afb7
[INF] Telafi adımı başarıyla tamamlandı: Ödeme işlemi. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
[INF] Telafi işlemleri tamamlandı. SagaId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

## Sonuç

Bu SAGA Pattern uygulaması, dağıtık işlemlerde tutarlılığı sağlamanın etkili bir yolunu göstermektedir. Gerçek bir uygulamada:

1. Her adım gerçek servislere bağlanabilir (ödeme servisi, stok yönetim servisi, vb.)
2. Tekrar denemeleri (retry) eklenebilir
3. Telafi işlemleri için daha karmaşık mantık uygulanabilir
4. Duruma göre kuyruk ve event mekanizmaları ile asenkron telafi işlemleri yapılabilir

SAGA Pattern, dağıtık mikroservis mimarilerinde tutarlılık ve hata toleransı sağlamak için önemli bir araçtır.
