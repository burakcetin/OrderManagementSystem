using Microsoft.EntityFrameworkCore;
using OrderManagement.Core.Entities;

namespace OrderManagement.Infrastructure.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasKey(o => o.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
