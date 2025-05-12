namespace SharedKernel.Wrappers
{
    /// <summary>
    /// Sayfalanmış veri için API yanıtı
    /// </summary>
    public class PagedResponse<T> : Response<T>
    {
        /// <summary>
        /// Sayfa numarası
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Sayfa boyutu
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Önceki sayfa var mı?
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
        
        /// <summary>
        /// Sonraki sayfa var mı?
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
        
        /// <summary>
        /// İlk sayfa
        /// </summary>
        public int FirstPage => 1;
        
        /// <summary>
        /// Son sayfa
        /// </summary>
        public int LastPage => TotalPages;
        
        /// <summary>
        /// Toplam sayfa sayısı
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// Toplam kayıt sayısı
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Sayfalanmış yanıt oluşturur
        /// </summary>
        public PagedResponse(T data, int pageNumber, int pageSize, int totalRecords)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            Data = data;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            Succeeded = true;
            Errors = null;
        }
    }
}
