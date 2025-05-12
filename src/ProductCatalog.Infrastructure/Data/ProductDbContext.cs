using Microsoft.EntityFrameworkCore;
using ProductCatalog.Core.Entities;

namespace ProductCatalog.Infrastructure.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);

            // Seed data
            SeedData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = Guid.Parse("9f8dd03e-1298-4070-aaf3-6b8dda98cfcc"),
                    Name = "Laptop Pro",
                    Description = "High performance laptop for professional use with the latest processor and graphics card.",
                    Price = 1299.99m,
                    Stock = 25,
                    Category = "Electronics",
                    ImageUrl = "/images/laptop-pro.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Product
                {
                    Id = Guid.Parse("35bd06e5-71db-4d1b-9a99-c6e12bbca8f2"),
                    Name = "Smartphone X",
                    Description = "Latest smartphone with a 6.5 inch screen, 128GB storage and triple camera setup.",
                    Price = 799.99m,
                    Stock = 50,
                    Category = "Electronics",
                    ImageUrl = "/images/smartphone-x.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = null
                },
                new Product
                {
                    Id = Guid.Parse("16e57930-10f1-4e4d-a7ae-95a19e9e36e9"),
                    Name = "Wireless Headphones",
                    Description = "Premium wireless headphones with noise cancellation and 20-hour battery life.",
                    Price = 249.99m,
                    Stock = 100,
                    Category = "Audio",
                    ImageUrl = "/images/wireless-headphones.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Product
                {
                    Id = Guid.Parse("4a73e9b5-3b0c-4078-ab91-4a872dc2f8a7"),
                    Name = "Smart Watch",
                    Description = "Smart watch with health monitoring, GPS and water resistance up to 50 meters.",
                    Price = 199.99m,
                    Stock = 75,
                    Category = "Wearables",
                    ImageUrl = "/images/smart-watch.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = null
                },
                new Product
                {
                    Id = Guid.Parse("db1d2c88-a7b3-48d5-b348-747f829b5f9d"),
                    Name = "Tablet Pro",
                    Description = "Ultra-slim tablet with 10.5 inch retina display, perfect for work and entertainment.",
                    Price = 499.99m,
                    Stock = 30,
                    Category = "Electronics",
                    ImageUrl = "/images/tablet-pro.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = null
                }
            );
        }
    }
}
