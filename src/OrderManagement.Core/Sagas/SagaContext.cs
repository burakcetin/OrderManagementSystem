using System;
using System.Collections.Generic;

namespace OrderManagement.Core.Sagas
{
    /// <summary>
    /// SAGA işlemi için bağlam bilgilerini tutar
    /// </summary>
    /// <typeparam name="T">SAGA işlemi veri tipi</typeparam>
    public class SagaContext<T> where T : class
    {
        /// <summary>
        /// SAGA işlemi benzersiz tanımlayıcısı
        /// </summary>
        public Guid SagaId { get; } = Guid.NewGuid();

        /// <summary>
        /// İşlem başlangıç zamanı
        /// </summary>
        public DateTime StartedAt { get; } = DateTime.UtcNow;

        /// <summary>
        /// SAGA işlemi verileri
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// SAGA adımlarının listesi
        /// </summary>
        public List<SagaStep<T>> Steps { get; } = new List<SagaStep<T>>();

        /// <summary>
        /// SAGA işlemine ait ek veriler
        /// </summary>
        public Dictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">SAGA işlemi verileri</param>
        public SagaContext(T data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// SAGA adımı ekler
        /// </summary>
        /// <param name="step">SAGA adımı</param>
        public void AddStep(SagaStep<T> step)
        {
            Steps.Add(step);
        }
    }
}
