using System;
using System.Collections.Generic;

namespace OrderManagement.Core.Sagas
{
    /// <summary>
    /// SAGA işlemi sonucu
    /// </summary>
    /// <typeparam name="T">SAGA veri tipi</typeparam>
    public class SagaResult<T> where T : class
    {
        /// <summary>
        /// SAGA işlem tanımlayıcısı
        /// </summary>
        public Guid SagaId { get; }

        /// <summary>
        /// İşlemin başarılı olup olmadığı
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// Güncel SAGA verileri
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// İşlemin tamamlandığı zaman
        /// </summary>
        public DateTime CompletedAt { get; }

        /// <summary>
        /// Tamamlanan adımlar 
        /// </summary>
        public List<string> CompletedSteps { get; }

        /// <summary>
        /// Başarısız olan adım (varsa)
        /// </summary>
        public string? FailedStep { get; }

        /// <summary>
        /// Hata mesajı (varsa)
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Constructor - Başarılı durum
        /// </summary>
        public SagaResult(Guid sagaId, T data, List<string> completedSteps)
        {
            SagaId = sagaId;
            Data = data;
            IsSuccessful = true;
            CompletedAt = DateTime.UtcNow;
            CompletedSteps = completedSteps;
            FailedStep = null;
            ErrorMessage = null;
        }

        /// <summary>
        /// Constructor - Başarısız durum
        /// </summary>
        public SagaResult(Guid sagaId, T data, List<string> completedSteps, string failedStep, string errorMessage)
        {
            SagaId = sagaId;
            Data = data;
            IsSuccessful = false;
            CompletedAt = DateTime.UtcNow;
            CompletedSteps = completedSteps;
            FailedStep = failedStep;
            ErrorMessage = errorMessage;
        }
    }
}
