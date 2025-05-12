using OrderManagement.Core.Entities;
using System.Threading.Tasks;

namespace OrderManagement.Core.Sagas.OrderSaga
{
    /// <summary>
    /// Sipariş SAGA Orchestrator arayüzü
    /// </summary>
    public interface IOrderSagaOrchestrator
    {
        /// <summary>
        /// Sipariş SAGA işlemini başlatır
        /// </summary>
        /// <param name="order">Sipariş bilgileri</param>
        /// <returns>SAGA sonucu</returns>
        Task<SagaResult<OrderSagaData>> ProcessOrderAsync(Order order);
    }
}
