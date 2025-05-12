namespace SharedKernel.Wrappers
{
    /// <summary>
    /// Generic veri içeren API yanıt sınıfı
    /// </summary>
    /// <typeparam name="T">Yanıtta döndürülecek veri tipi</typeparam>
    public class Response<T> : Response
    {
        /// <summary>
        /// Yanıtta döndürülecek veri
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Boş yanıt oluşturur
        /// </summary>
        public Response()
        {
        }
        
        /// <summary>
        /// Veri ile başarılı yanıt oluşturur
        /// </summary>
        public Response(T data)
        {
            Succeeded = true;
            Data = data;
        }

        /// <summary>
        /// Veri içeren başarılı yanıt oluşturur
        /// </summary>
        public static Response<T> Success(T data)
        {
            return new Response<T> { Succeeded = true, Data = data };
        }

        /// <summary>
        /// Veri ve mesaj içeren başarılı yanıt oluşturur
        /// </summary>
        public static Response<T> Success(T data, string message)
        {
            return new Response<T> { Succeeded = true, Data = data, Message = message };
        }

        /// <summary>
        /// Generic hata yanıtı oluşturur
        /// </summary>
        public static new Response<T> Fail()
        {
            return new Response<T> { Succeeded = false };
        }
        
        /// <summary>
        /// Mesajlı generic hata yanıtı oluşturur
        /// </summary>
        public static new Response<T> Fail(string message)
        {
            return new Response<T> { Succeeded = false, Message = message };
        }
        
        /// <summary>
        /// Hata mesajları listesi ile generic hata yanıtı oluşturur
        /// </summary>
        public static new Response<T> Fail(string[] errors)
        {
            return new Response<T> { Succeeded = false, Errors = errors };
        }
        
        /// <summary>
        /// Hata mesajları listesi ve mesaj ile generic hata yanıtı oluşturur
        /// </summary>
        public static new Response<T> Fail(string message, string[] errors)
        {
            return new Response<T> { Succeeded = false, Message = message, Errors = errors };
        }

        /// <summary>
        /// Data içeren başarısız yanıt oluşturur
        /// </summary>
        public static Response<T> Fail(T data)
        {
            return new Response<T> { Succeeded = false, Data = data };
        }

        /// <summary>
        /// Veri ve mesaj içeren başarısız yanıt oluşturur
        /// </summary>
        public static Response<T> Fail(T data, string message)
        {
            return new Response<T> { Succeeded = false, Data = data, Message = message };
        }

        /// <summary>
        /// Veri ve hata listesi içeren başarısız yanıt oluşturur
        /// </summary>
        public static Response<T> Fail(T data, string[] errors)
        {
            return new Response<T> { Succeeded = false, Data = data, Errors = errors };
        }

        /// <summary>
        /// Veri, mesaj ve hata listesi içeren başarısız yanıt oluşturur
        /// </summary>
        public static Response<T> Fail(T data, string message, string[] errors)
        {
            return new Response<T> { Succeeded = false, Data = data, Message = message, Errors = errors };
        }
    }
}
