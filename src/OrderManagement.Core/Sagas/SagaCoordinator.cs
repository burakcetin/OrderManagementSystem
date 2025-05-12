using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderManagement.Core.Sagas
{
    /// <summary>
    /// SAGA işlemlerini koordine eden sınıf
    /// </summary>
    public class SagaCoordinator : ISagaCoordinator
    {
        private readonly ILogger<SagaCoordinator> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logger</param>
        public SagaCoordinator(ILogger<SagaCoordinator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// SAGA işlemini çalıştırır
        /// </summary>
        public async Task<SagaResult<T>> ExecuteSagaAsync<T>(SagaContext<T> context) where T : class
        {
            _logger.LogInformation("SAGA başlatılıyor. SagaId: {SagaId}", context.SagaId);
            
            var completedSteps = new List<string>();
            var executedStepCount = 0;

            try
            {
                foreach (var step in context.Steps)
                {
                    _logger.LogInformation("SAGA adımı çalıştırılıyor: {StepDescription}. SagaId: {SagaId}",
                        step.Description, context.SagaId);
                    
                    var success = await step.ExecuteAction(context.Data);
                    executedStepCount++;
                    
                    if (success)
                    {
                        completedSteps.Add(step.Description);
                        _logger.LogInformation("SAGA adımı başarıyla tamamlandı: {StepDescription}. SagaId: {SagaId}",
                            step.Description, context.SagaId);
                    }
                    else
                    {
                        _logger.LogWarning("SAGA adımı başarısız oldu: {StepDescription}. SagaId: {SagaId}. Telafi işlemleri başlatılıyor.",
                            step.Description, context.SagaId);
                        
                        await RollbackAsync(context, executedStepCount - 1);
                        
                        return new SagaResult<T>(
                            context.SagaId,
                            context.Data,
                            completedSteps,
                            step.Description,
                            $"SAGA adımı başarısız oldu: {step.Description}"
                        );
                    }
                }

                _logger.LogInformation("SAGA başarıyla tamamlandı. SagaId: {SagaId}", context.SagaId);
                return new SagaResult<T>(context.SagaId, context.Data, completedSteps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SAGA çalışırken hata oluştu. SagaId: {SagaId}. Telafi işlemleri başlatılıyor.", context.SagaId);
                
                await RollbackAsync(context, executedStepCount - 1);
                
                var failedStep = executedStepCount > 0 && executedStepCount <= context.Steps.Count
                    ? context.Steps[executedStepCount - 1].Description
                    : "Bilinmeyen adım";
                
                return new SagaResult<T>(
                    context.SagaId,
                    context.Data,
                    completedSteps,
                    failedStep,
                    $"SAGA işleminde hata: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Telafi işlemlerini başarısızlık durumunda çalıştırır
        /// </summary>
        private async Task RollbackAsync<T>(SagaContext<T> context, int lastCompletedStepIndex) where T : class
        {
            for (int i = lastCompletedStepIndex; i >= 0; i--)
            {
                var step = context.Steps[i];
                _logger.LogInformation("Telafi adımı çalıştırılıyor: {StepDescription}. SagaId: {SagaId}",
                    step.Description, context.SagaId);
                
                try
                {
                    var success = await step.CompensateAction(context.Data);
                    if (success)
                    {
                        _logger.LogInformation("Telafi adımı başarıyla tamamlandı: {StepDescription}. SagaId: {SagaId}",
                            step.Description, context.SagaId);
                    }
                    else
                    {
                        _logger.LogWarning("Telafi adımı başarısız oldu: {StepDescription}. SagaId: {SagaId}",
                            step.Description, context.SagaId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Telafi adımı çalıştırılırken hata oluştu: {StepDescription}. SagaId: {SagaId}",
                        step.Description, context.SagaId);
                }
            }

            _logger.LogInformation("Telafi işlemleri tamamlandı. SagaId: {SagaId}", context.SagaId);
        }
    }
}
