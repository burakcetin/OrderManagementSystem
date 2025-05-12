using System;
using System.Threading.Tasks;

namespace OrderManagement.Core.Sagas
{
    /// <summary>
    /// SAGA işleminin bir adımını temsil eder
    /// </summary>
    /// <typeparam name="T">SAGA veri tipi</typeparam>
    public class SagaStep<T> where T : class
    {
        /// <summary>
        /// Adımın açıklaması
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Adımın çalıştırılacak asıl işlemi
        /// </summary>
        public Func<T, Task<bool>> ExecuteAction { get; }

        /// <summary>
        /// Adımın telafi (compensation) işlemi - başarısızlık durumunda çalıştırılır
        /// </summary>
        public Func<T, Task<bool>> CompensateAction { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">Adım açıklaması</param>
        /// <param name="executeAction">Çalıştırılacak asıl işlem</param>
        /// <param name="compensateAction">Telafi işlemi</param>
        public SagaStep(
            string description,
            Func<T, Task<bool>> executeAction,
            Func<T, Task<bool>> compensateAction)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));
            ExecuteAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            CompensateAction = compensateAction ?? throw new ArgumentNullException(nameof(compensateAction));
        }
    }
}
