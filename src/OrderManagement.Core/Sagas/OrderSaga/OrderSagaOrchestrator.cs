using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Core.Commands.SagaCommands;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using OrderManagement.Core.Sagas;
using SharedKernel.Commands.Stock;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderManagement.Core.Sagas.OrderSaga
{
    /// <summary>
    /// Sipariş SAGA Orchestrator implementasyonu - CQRS yapısı kullanılarak
    /// </summary>
    public class OrderSagaOrchestrator : IOrderSagaOrchestrator
    {
        private readonly ISagaCoordinator _sagaCoordinator;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<OrderSagaOrchestrator> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public OrderSagaOrchestrator(
            ISagaCoordinator sagaCoordinator,
            IOrderRepository orderRepository,
            IMediator mediator,
            ILogger<OrderSagaOrchestrator> logger)
        {
            _sagaCoordinator = sagaCoordinator;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Sipariş SAGA işlemini oluşturur ve çalıştırır
        /// </summary>
        public async Task<SagaResult<OrderSagaData>> ProcessOrderAsync(Order order)
        {
            _logger.LogInformation("Sipariş SAGA işlemi başlatılıyor. OrderId: {OrderId}", order.Id);

            var productDetailsQuery = new GetProductDetailsQuery
            {
                ProductId = order.ProductId ?? default
            };
            
            var productDetails = await _mediator.Send(productDetailsQuery);
            
            if (!productDetails.IsSuccess)
            {
                _logger.LogWarning("SAGA işlemi başlatılamadı: {ErrorMessage}. ProductId: {ProductId}", 
                    productDetails.ErrorMessage, order.ProductId);
                
                order.Status = OrderStatus.Failed;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);
                
                var errorData = new OrderSagaData
                {
                    OrderId = order.Id,
                    ProductId = order.ProductId ?? default,
                    OrderDetails = order
                };
                errorData.AddStepLog($"Ürün bilgisi alınamadı: {productDetails.ErrorMessage}");
                
                return new SagaResult<OrderSagaData>(
                    Guid.NewGuid(),
                    errorData,
                    new List<string>(),
                    "Initial Check",
                    productDetails.ErrorMessage
                );
            }

            var sagaData = new OrderSagaData
            {
                OrderId = order.Id,
                ProductId = order.ProductId ?? default,
                OrderDetails = order
            };

            var sagaContext = new SagaContext<OrderSagaData>(sagaData);

            sagaContext.AddStep(new SagaStep<OrderSagaData>(
                "Ödeme işlemi",
                async (data) =>
                {
                    try
                    {
                        var processPaymentCommand = new ProcessPaymentCommand
                        {
                            OrderId = data.OrderId,
                            Amount = data.OrderDetails.Price * data.OrderDetails.Quantity,
                            CustomerName = data.OrderDetails.CustomerName,
                            CustomerEmail = data.OrderDetails.CustomerEmail
                        };
                        
                        var paymentResult = await _mediator.Send(processPaymentCommand);
                        
                        if (!paymentResult.IsSuccess)
                        {
                            _logger.LogWarning("Ödeme başarısız oldu. OrderId: {OrderId}, Hata: {ErrorMessage}", 
                                data.OrderId, paymentResult.ErrorMessage);
                            
                            data.AddStepLog($"Ödeme başarısız: {paymentResult.ErrorMessage}");
                            return false;
                        }
                        
                        data.PaymentTransactionId = paymentResult.TransactionId;
                        data.AddStepLog($"Ödeme başarılı. İşlem ID: {paymentResult.TransactionId}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ödeme işlemi sırasında hata oluştu. OrderId: {OrderId}", data.OrderId);
                        data.AddStepLog($"Ödeme işleminde hata: {ex.Message}");
                        return false;
                    }
                },
                async (data) =>
                {
                    if (data.PaymentTransactionId.HasValue)
                    {
                        _logger.LogInformation("Ödeme iadesi yapılıyor. İşlem ID: {PaymentId}, OrderId: {OrderId}",
                            data.PaymentTransactionId, data.OrderId);
                        
                        var refundPaymentCommand = new RefundPaymentCommand
                        {
                            PaymentTransactionId = data.PaymentTransactionId.Value,
                            Amount = data.OrderDetails.Price * data.OrderDetails.Quantity
                        };
                        
                        var refundResult = await _mediator.Send(refundPaymentCommand);
                        
                        if (refundResult.IsSuccess)
                        {
                            data.AddStepLog($"Ödeme iadesi yapıldı. İşlem ID: {data.PaymentTransactionId}, İade ID: {refundResult.RefundTransactionId}");
                            data.PaymentTransactionId = null;
                        }
                        else
                        {
                            _logger.LogWarning("Ödeme iadesi başarısız oldu. İşlem ID: {PaymentId}, OrderId: {OrderId}, Hata: {ErrorMessage}",
                                data.PaymentTransactionId, data.OrderId, refundResult.ErrorMessage);
                            data.AddStepLog($"Ödeme iadesi başarısız oldu. İşlem ID: {data.PaymentTransactionId}, Hata: {refundResult.ErrorMessage}");
                            // İade başarısız olsa da devam et, manuel müdahale gerekebilir
                        }
                    }
                    
                    return true;
                }
            ));

            sagaContext.AddStep(new SagaStep<OrderSagaData>(
                "Stok düşme",
                async (data) =>
                {
                    try
                    {
                        var reduceStockCommand = new ReduceStockCommand
                        {
                            ProductId = data.ProductId,
                            Quantity = data.OrderDetails.Quantity
                        };
                        
                        var stockResult = await _mediator.Send(reduceStockCommand);
                        
                        if (!stockResult.IsSuccess)
                        {
                            _logger.LogWarning("Stok düşme işlemi başarısız oldu. ProductId: {ProductId}, OrderId: {OrderId}, Hata: {ErrorMessage}",
                                data.ProductId, data.OrderId, stockResult.ErrorMessage);
                            
                            data.AddStepLog($"Stok düşme işlemi başarısız: {stockResult.ErrorMessage}");
                            return false;
                        }
                        
                        data.StockReduced = true;
                        data.ReducedStockQuantity = stockResult.ReducedQuantity;
                        data.AddStepLog($"Stok düşüldü. Ürün: {data.ProductId}, Miktar: {stockResult.ReducedQuantity}, Yeni Stok: {stockResult.NewStockLevel}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Stok düşme işlemi sırasında hata oluştu. OrderId: {OrderId}, ProductId: {ProductId}",
                            data.OrderId, data.ProductId);
                        data.AddStepLog($"Stok düşme işleminde hata: {ex.Message}");
                        return false;
                    }
                },
                async (data) =>
                {
                    if (data.StockReduced)
                    {
                        _logger.LogInformation("Stok geri ekleniyor. ProductId: {ProductId}, Miktar: {Quantity}, OrderId: {OrderId}",
                            data.ProductId, data.ReducedStockQuantity, data.OrderId);
                        
                        var revertStockCommand = new RevertStockCommand
                        {
                            ProductId = data.ProductId,
                            Quantity = data.ReducedStockQuantity
                        };
                        
                        var revertResult = await _mediator.Send(revertStockCommand);
                        
                        if (revertResult.IsSuccess)
                        {
                            data.AddStepLog($"Stok geri eklendi. Ürün: {data.ProductId}, Miktar: {revertResult.RestoredQuantity}, Yeni Stok: {revertResult.NewStockLevel}");
                            data.StockReduced = false;
                            data.ReducedStockQuantity = 0;
                        }
                        else
                        {
                            _logger.LogWarning("Stok geri ekleme işlemi başarısız oldu. ProductId: {ProductId}, Miktar: {Quantity}, Hata: {ErrorMessage}",
                                data.ProductId, data.ReducedStockQuantity, revertResult.ErrorMessage);
                            data.AddStepLog($"Stok geri ekleme işlemi başarısız. Ürün: {data.ProductId}, Miktar: {data.ReducedStockQuantity}, Hata: {revertResult.ErrorMessage}");
                            // Stok geri ekleme başarısız olsa da devam et, manuel müdahale gerekebilir
                        }
                    }
                    
                    return true;
                }
            ));

            sagaContext.AddStep(new SagaStep<OrderSagaData>(
                "Sipariş güncelleme",
                async (data) =>
                {
                    try
                    {
                        _logger.LogInformation("Sipariş durumu güncelleniyor. OrderId: {OrderId}", data.OrderId);

                        data.OrderDetails.Status = OrderStatus.Processing;
                        data.OrderDetails.UpdatedAt = DateTime.UtcNow;
                        
                        await _orderRepository.UpdateAsync(data.OrderDetails);
                        data.AddStepLog($"Sipariş durumu güncellendi: {data.OrderDetails.Status}");
                        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Sipariş güncellemesi sırasında hata oluştu. OrderId: {OrderId}", data.OrderId);
                        data.AddStepLog($"Sipariş güncellemesi sırasında hata: {ex.Message}");
                        return false;
                    }
                },
                async (data) =>
                {
                    try
                    {
                        _logger.LogInformation("Sipariş başarısız olarak işaretleniyor. OrderId: {OrderId}", data.OrderId);

                        data.OrderDetails.Status = OrderStatus.Failed;
                        data.OrderDetails.UpdatedAt = DateTime.UtcNow;
                        
                        await _orderRepository.UpdateAsync(data.OrderDetails);
                        data.AddStepLog($"Sipariş durumu Failed olarak güncellendi");
                        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Sipariş telafi güncellemesi sırasında hata oluştu. OrderId: {OrderId}", data.OrderId);
                        data.AddStepLog($"Sipariş telafi güncellemesi sırasında hata: {ex.Message}");
                        return false;
                    }
                }
            ));

            var result = await _sagaCoordinator.ExecuteSagaAsync(sagaContext);

            if (result.IsSuccessful)
            {
                try
                {
                    sagaData.OrderDetails.Status = OrderStatus.Shipped;
                    sagaData.OrderDetails.UpdatedAt = DateTime.UtcNow;
                    await _orderRepository.UpdateAsync(sagaData.OrderDetails);
                    sagaData.AddStepLog($"Sipariş başarıyla tamamlandı. Durumu güncellendi: {sagaData.OrderDetails.Status}");
                    
                    _logger.LogInformation("Sipariş başarıyla tamamlandı. OrderId: {OrderId}, Yeni Durum: {Status}", 
                        sagaData.OrderId, sagaData.OrderDetails.Status);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SAGA sonrası sipariş güncellemesi sırasında hata oluştu. OrderId: {OrderId}", order.Id);
                    sagaData.AddStepLog($"Final güncelleme sırasında hata: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("Sipariş SAGA işlemi başarısız oldu. OrderId: {OrderId}, Hata: {ErrorMessage}",
                    order.Id, result.ErrorMessage);
            }

            return result;
        }
    }
}
