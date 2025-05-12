using System;
using System.Threading.Tasks;

namespace OrderManagement.Core.Sagas
{
    /// <summary>
    /// SAGA koordinatör arayüzü - SAGA işlemlerini yönetir
    /// </summary>
    public interface ISagaCoordinator
    {
        /// <summary>
        /// SAGA işlemini başlatır
        /// </summary>
        /// <param name="context">SAGA bağlamı</param>
        /// <typeparam name="T">SAGA veri tipi</typeparam>
        /// <returns>SAGA sonucu</returns>
        Task<SagaResult<T>> ExecuteSagaAsync<T>(SagaContext<T> context) where T : class;
    }
}
