using OrderManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderManagement.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid id);
        Task<List<Order>> GetAllAsync();
        Task<Order> AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task<bool> DeleteAsync(Guid id);
    }
}
