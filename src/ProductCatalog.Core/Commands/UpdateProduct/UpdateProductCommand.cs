using MediatR;
using System;

namespace ProductCatalog.Core.Commands.UpdateProduct
{
    /// <summary>
    /// Ürün güncelleme komutu
    /// </summary>
    public class UpdateProductCommand : IRequest<UpdateProductCommandResponse>
    {
        /// <summary>
        /// Ürün ID
        /// </summary>
        public Guid ProductId { get; set; }
        
        /// <summary>
        /// Ürün Adı
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Ürün Açıklaması
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Ürün Fiyatı
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Stok Miktarı
        /// </summary>
        public int Stock { get; set; }
        
        /// <summary>
        /// Ürün Kategorisi
        /// </summary>
        public string Category { get; set; }
    }
    
    /// <summary>
    /// Ürün güncelleme yanıtı
    /// </summary>
    public class UpdateProductCommandResponse
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// Mesaj
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Sonuç
        /// </summary>
        public UpdateProductCommandResult Data { get; set; }
        
        /// <summary>
        /// Başarılı yanıt oluşturur
        /// </summary>
        public static UpdateProductCommandResponse Success(UpdateProductCommandResult data, string message = null)
        {
            return new UpdateProductCommandResponse
            {
                Succeeded = true,
                Data = data,
                Message = message
            };
        }
        
        /// <summary>
        /// Başarısız yanıt oluşturur
        /// </summary>
        public static UpdateProductCommandResponse Fail(string message)
        {
            return new UpdateProductCommandResponse
            {
                Succeeded = false,
                Message = message
            };
        }
    }
    
    /// <summary>
    /// Ürün güncelleme sonucu
    /// </summary>
    public class UpdateProductCommandResult
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Ürün ID
        /// </summary>
        public Guid ProductId { get; set; }
        
        /// <summary>
        /// Ürün Adı
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Ürün Fiyatı
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Stok Miktarı
        /// </summary>
        public int Stock { get; set; }
    }
}
