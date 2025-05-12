namespace SharedKernel.Wrappers
{
    /// <summary>
    /// Tüm API yanıtlarının temel sınıfı
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Yanıt başarılı mı?
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// Hata mesajları listesi
        /// </summary>
        public string[] Errors { get; set; }
        
        /// <summary>
        /// Mesaj
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Boş başarılı yanıt oluşturur
        /// </summary>
        public static Response Success()
        {
            return new Response { Succeeded = true };
        }
        
        /// <summary>
        /// Mesajlı başarılı yanıt oluşturur
        /// </summary>
        public static Response Success(string message)
        {
            return new Response { Succeeded = true, Message = message };
        }
        
        /// <summary>
        /// Hata yanıtı oluşturur
        /// </summary>
        public static Response Fail()
        {
            return new Response { Succeeded = false };
        }
        
        /// <summary>
        /// Mesajlı hata yanıtı oluşturur
        /// </summary>
        public static Response Fail(string message)
        {
            return new Response { Succeeded = false, Message = message };
        }
        
        /// <summary>
        /// Hata mesajları listesi ile hata yanıtı oluşturur
        /// </summary>
        public static Response Fail(string[] errors)
        {
            return new Response { Succeeded = false, Errors = errors };
        }
        
        /// <summary>
        /// Hata mesajları listesi ve mesaj ile hata yanıtı oluşturur
        /// </summary>
        public static Response Fail(string message, string[] errors)
        {
            return new Response { Succeeded = false, Message = message, Errors = errors };
        }
    }
}
