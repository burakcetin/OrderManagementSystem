using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using OrderManagement.Core.Messages;
using OrderManagement.Core.Sagas.OrderSaga;
using OrderManagement.Core.Services;
using Rebus.Handlers;
using System;
using System.Threading.Tasks;

namespace OrderManagement.WorkerService.Handlers
{
    /// <summary>
    /// Rebus mesaj handler'ı - OrderCreatedMessage mesajını işler
    /// SAGA pattern uygulayarak sipariş işlemlerini yürütür
    /// </summary>
    public class OrderCreatedMessageHandler : IHandleMessages<OrderCreatedMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderCreatedMessageHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public OrderCreatedMessageHandler(
            IServiceProvider serviceProvider,
            IEmailService emailService,
            ILogger<OrderCreatedMessageHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Mesajı işler - SAGA Pattern ile Sipariş Sürecini Yönetir
        /// 1. Sipariş Kaydet
        /// 2. Ürün Stoğunu Azalt
        /// 3. Email Gönder
        /// </summary>
        public async Task Handle(OrderCreatedMessage message)
        {
            _logger.LogInformation("OrderCreatedMessage alındı: {OrderId}, {ProductName}, {Price}, {CustomerEmail}",
                message.OrderId, message.ProductName, message.Price, message.CustomerEmail);

            using (var scope = _serviceProvider.CreateScope())
            {
                var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                var sagaOrchestrator = scope.ServiceProvider.GetRequiredService<IOrderSagaOrchestrator>();

 
                    // Siparişi veritabanından al (API tarafından "Pending" durumunda kaydedilmiş olmalı)
                    var order = await orderRepository.GetByIdAsync(message.OrderId);
                    
                    if (order == null)
                    {
                        _logger.LogError("Sipariş veritabanında bulunamadı. OrderId: {OrderId}", message.OrderId);
                        return;
                    }
                    
                    // Sipariş durumunu Created olarak güncelle
                    order.Status = OrderStatus.Created;
                    order.UpdatedAt = DateTime.UtcNow;
                    await orderRepository.UpdateAsync(order);
                    _logger.LogInformation("Sipariş durumu 'Created' olarak güncellendi. OrderId: {OrderId}", order.Id);

                    // SAGA işlemini başlat
                    _logger.LogInformation("Sipariş için SAGA işlemi başlatılıyor. OrderId: {OrderId}", order.Id);
                    var result = await sagaOrchestrator.ProcessOrderAsync(order);

                    if (result.IsSuccessful)
                    {
                        _logger.LogInformation("Sipariş SAGA işlemi başarıyla tamamlandı. OrderId: {OrderId}", order.Id);

                        foreach (var completedStep in result.CompletedSteps)
                        {
                            _logger.LogInformation("Tamamlanan SAGA adımı: {Step}. OrderId: {OrderId}",
                                completedStep, order.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Sipariş SAGA işlemi başarısız oldu. OrderId: {OrderId}, Başarısız Adım: {FailedStep}, Hata: {Error}",
                            order.Id, result.FailedStep, result.ErrorMessage);

                        // Durumu Failed olarak güncelle
                        order.Status = OrderStatus.Failed;
                        order.UpdatedAt = DateTime.UtcNow;
                        await orderRepository.UpdateAsync(order);
                        _logger.LogInformation("Sipariş durumu 'Failed' olarak güncellendi. OrderId: {OrderId}", order.Id);

                        foreach (var completedStep in result.CompletedSteps)
                        {
                            _logger.LogInformation("Tamamlanan SAGA adımı: {Step}. OrderId: {OrderId}",
                                completedStep, order.Id);
                        }

                        if (result.Data?.StepLogs != null)
                        {
                            foreach (var log in result.Data.StepLogs)
                            {
                                _logger.LogInformation("SAGA log: {Log}. OrderId: {OrderId}", log, order.Id);
                            }
                        }
                    }
                
            }
        }
    }
}