# OrderManagementSystem Projesi

## Proje Özeti

OrderManagementSystem, modern bir sipariş yönetim sistemidir. Projede Clean Architecture, CQRS, Saga Pattern, Event-Driven Architecture, Microservices ve container teknolojileri kullanılmıştır. Bu sistem, sipariş oluşturma, stok yönetimi, ödeme işlemleri ve sipariş takibi gibi temel e-ticaret operasyonlarını destekler.

## Proje Mimarisi

### Temel Yapı

Proje aşağıdaki katmanlı mimariye sahiptir:

1. **API Katmanı**: HTTP isteklerini karşılar, istekleri komutlara ve sorgulara dönüştürür
2. **Core Katmanı**: İş mantığını içerir; varlıklar, komutlar, sorgular ve domain servisleri burada bulunur
3. **Infrastructure Katmanı**: Veritabanı, mesajlaşma, harici servisler gibi altyapısal bileşenleri içerir
4. **WorkerService**: Arka planda çalışan ve mesaj tabanlı işlemleri gerçekleştiren servis

### Kullanılan Teknolojiler ve Kütüphaneler

- **.NET 6**: Projenin temel framework'ü
- **Redis**: Hızlı veri okuma/yazma işlemleri için kullanılan NoSQL veritabanı
- **Entity Framework Core**: Veritabanı işlemleri için ORM
- **RabbitMQ / Rebus**: Mesaj kuyruk altyapısı
- **MediatR**: CQRS yapısını uygulamak için kullanılan kütüphane
- **FluentValidation**: Veri doğrulama için kullanılan kütüphane
- **Docker**: Konteynerizasyon için kullanılan platform
- **Grafana/Loki**: Loglama ve izleme altyapısı

## Tasarım Desenleri ve Mimari Yaklaşımlar

### 1. Clean Architecture

Proje, Clean Architecture prensiplerine göre yapılandırılmıştır:

- **Domain Layer (Core)**: Tüm iş mantığını içerir
- **Application Layer (Core)**: Uygulamanın davranışını ve akışını yönetir
- **Infrastructure Layer**: Harici sistemlerle ve veritabanıyla iletişimi sağlar
- **API/UI Layer**: Kullanıcı arayüzlerini ve API'leri sunar

Bu yaklaşım sayesinde, sistemin çekirdeği dış dünyaya bağımlı değildir; bağımlılıklar her zaman dıştan içe doğrudur.

### 2. CQRS (Command Query Responsibility Segregation)

Proje, MediatR kütüphanesi kullanılarak CQRS desenini uygular:

- **Commands**: Veri değiştiren işlemler için (örn. CreateOrderCommand)
- **Queries**: Veri okuyan işlemler için (örn. GetOrderByIdQuery)

Bu yaklaşım:
- İş mantığını daha iyi organize etmeyi
- Yazma ve okuma işlemlerini ayırmayı
- Her bir işlemi tek bir sorumlulukta tutmayı sağlar

### 3. Saga Pattern

Sistem, dağıtık işlemleri koordine etmek için Saga Pattern'ı kullanır:

- **OrderSagaOrchestrator**: Sipariş işlem adımlarını koordine eder
- **SagaCoordinator**: Genel SAGA işlemlerini yönetir
- **SagaStep**: Her bir işlem adımını ve telafi edici aksiyonları içerir

Bu desen sayesinde:
- Birden fazla mikroservis arasında işlem tutarlılığı sağlanır
- Herhangi bir aşamada hata olursa, önceki adımlar telafi edici aksiyonlarla geri alınır
- Uzun süreli işlemler atomik parçalara bölünür

### 4. Event-Driven Architecture

Sistem, olaylar (event) üzerinden iletişim kuran bir yapıya sahiptir:

- Siparişler oluşturulduğunda `OrderCreatedMessage` olayı yayınlanır
- Worker servis bu olayları dinler ve gerekli işlemleri başlatır
- RabbitMQ ve Rebus kütüphanesi kullanılarak mesaj alışverişi sağlanır

Bu yaklaşım:
- Sistemin bileşenlerini gevşek bağlı (loosely coupled) tutar
- Asenkron iletişim sağlar
- Sistemin ölçeklenebilirliğini artırır

### 5. Repository Pattern

Veri erişimi için Repository Pattern kullanılmıştır:

- **IOrderRepository**: Sipariş verileri için soyut arayüz
- **RedisOrderRepository**: Redis veritabanı için somut implementasyon

Bu desen:
- Veritabanı erişim kodunu izole eder
- Farklı veritabanı teknolojileri arasında geçişi kolaylaştırır
- Test edilebilirliği artırır

### 6. Dependency Injection

Sistem, bağımlılıkların yönetimi için Dependency Injection (DI) kullanır:

- Servisler, repository'ler ve diğer bileşenler constructor injection ile enjekte edilir
- .NET Core'un built-in DI container'ı kullanılır

Bu yaklaşım:
- Gevşek bağlı bileşenler oluşturur
- Test edilebilirliği artırır
- Kodun bakımını kolaylaştırır

## Sipariş İşleme Akışı

Sistemdeki tipik bir sipariş akışı şu şekildedir:

1. Kullanıcı API üzerinden sipariş oluşturma isteği gönderir
2. `CreateOrderCommandHandler` siparişi veritabanına kaydeder ve `OrderCreatedMessage` olayını yayınlar
3. WorkerService olayı dinler ve `OrderCreatedMessageHandler` üzerinden işlemeye başlar
4. `OrderSagaOrchestrator` sipariş işleme akışını başlatır:
   - Ödeme işlemi yapılır
   - Ürün stoku düşürülür
   - Sipariş durumu güncellenir
5. Herhangi bir adımda hata olursa, telafi edici aksiyonlar devreye girer:
   - Ödeme iade edilir
   - Stok düzeltilir
   - Sipariş durumu "Failed" olarak işaretlenir
6. Tüm adımlar başarılı olursa sipariş "Shipped" durumuna geçer

## Veritabanı Yaklaşımı

Sistem, yüksek performans için Redis kullanır:

- Siparişler ve ürünler Redis'te JSON formatında saklanır
- Her entity tipi için Redis key pattern'ları kullanılır (örn. "order:{id}")
- `IRedisRepository<T>` genel bir Redis erişim katmanı sağlar

Bu yaklaşım:
- Hızlı okuma/yazma performansı sağlar
- Yüksek erişilebilirlik ve ölçeklenebilirlik sunar
- Cache ve veritabanı işlevlerini birleştirir

## API Tasarımı

API, RESTful prensiplerine uygun olarak tasarlanmıştır:

- **GET /api/orders**: Tüm siparişleri listeler
- **GET /api/orders/{id}**: Belirli bir siparişi getirir
- **GET /api/orders/{id}/status**: Sipariş durumunu getirir
- **POST /api/orders**: Yeni sipariş oluşturur
- **PUT /api/orders/{id}/status**: Sipariş durumunu günceller

API özellikleri:
- Swagger UI ile dokümantasyon
- HTTP status kodları ile uygun hata yönetimi
- CQRS için MediatR entegrasyonu
- ApiResponseWrapper ile tüm yanıtların standart bir formatta döndürülmesi, böylece istemci tarafında tutarlı yanıt işleme ve hata yönetimi sağlanır

## Hata Yönetimi ve Loglama

Sistem kapsamlı hata yönetimi ve loglama altyapısına sahiptir:

- Tüm işlemler loglanır (işlem başlangıcı, bitişi, hatalar)
- Loki/Grafana ile loglama ve izleme yapılır
- Exception handling middleware ile API hataları yönetilir
- SAGA pattern'ı ile işlem hataları telafi edilir

## Container ve Deployment

Proje Docker konteynerlerinde çalışacak şekilde yapılandırılmıştır:

- Her mikroservis için ayrı Dockerfile
- docker-compose.yml ile tüm sistemi tek komutla ayağa kaldırma
- Redis, RabbitMQ gibi altyapı servislerinin konteynerize edilmesi

## Geliştirme İçin Notlar

1. Projeyi klonlayın
2. Docker Desktop'ı yükleyin ve çalıştırın
3. Proje dizininde `docker-compose up -d` komutunu çalıştırın
4. API'lere aşağıdaki URL'ler üzerinden erişebilirsiniz:
   - Order API: http://localhost:8005/swagger
   - Product API: http://localhost:8002/swagger
   - Redis Commander: http://localhost:8081
   - Grafana: http://localhost:3000 (kullanıcı adı: admin, şifre: admin)
   - RabbitMQ Management: http://localhost:15672 (kullanıcı adı: guest, şifre: guest)

## İlave Notlar

- Proje, mikroservis mimarisine uygun şekilde genişletilmeye hazırdır
- Gerçek bir ödeme gateway'i için `PaymentService` sınıfı adapte edilebilir
- Yük testi ve performans optimizasyonu için Redis yapılandırması ince ayar gerektirebilir
- Yeni siparişler için e-posta bildirimleri `IEmailService` üzerinden implemente edilebilir