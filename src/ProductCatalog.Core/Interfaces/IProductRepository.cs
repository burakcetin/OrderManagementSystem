using ProductCatalog.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductCatalog.Core.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(Guid id);
        Task<List<Product>> GetAllAsync();
        Task<List<Product>> GetByCategoryAsync(string category);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task<bool> DeleteAsync(Guid id);
    }
}
