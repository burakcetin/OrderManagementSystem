using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MediatR; 
using OrderManagement.Infrastructure.Logging;
using ProductCatalog.Core.Behaviors;
using ProductCatalog.Core.Queries.GetAllProducts;
using ProductCatalog.Core.Interfaces;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Repositories;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;  
using ProductCatalog.Core.Commands.UpdateProduct;
using SharedKernel.Api;
using SharedKernel.Commands.Stock;
using SharedKernel.Data.Redis;
using ProductCatalog.Core.Entities;
using ProductCatalog.API.DataMigration;
var builder = WebApplication.CreateBuilder(args);

// Configure Loki logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Still keep console for local debugging
builder.Logging.AddLoki(options => 
{
    options.LokiUrl = builder.Configuration["Logging:Loki:Url"] ?? "http://loki:3100";
    options.DefaultLabels.Add("Application", "product-catalog-api");
    options.DefaultLabels.Add("Environment", builder.Environment.EnvironmentName);
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use camelCase for JSON property names
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        
        // Handle enums as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        
        // Ignore null values
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        
        // Pretty print JSON in development
        if (builder.Environment.IsDevelopment())
        {
            options.JsonSerializerOptions.WriteIndented = true;
        }
    });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product Catalog API",
        Version = "v1",
        Description = "API for product catalog management",
        Contact = new OpenApiContact
        {
            Name = "DigiCase",
            Email = "info@example.com"
        }
    });

    // XML dosyalarını ekleyelim (controller ve modellerde XML yorum yazarsak gösterilecek)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Redis services
builder.Services.AddRedisServices(builder.Configuration);
builder.Services.AddRedisRepository<Product>("product");

// Register repositories
// builder.Services.AddScoped<IProductRepository, ProductRepository>(); // InMemory repository - deaktif
builder.Services.AddScoped<IProductRepository, RedisProductRepository>(); // Redis repository

// Register MediatR
builder.Services.AddMediatR(cfg => 
{
    // Register both queries and commands
    cfg.RegisterServicesFromAssembly(typeof(GetAllProductsQuery).Assembly);
     cfg.RegisterServicesFromAssembly(typeof(RevertStockCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(UpdateProductCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// Register validators
builder.Services.AddValidatorsFromAssembly(typeof(GetAllProductsQuery).Assembly);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Register seed helper
builder.Services.AddScoped<SeedProductsToRedis>();

var app = builder.Build();

// Statik dosyaları sunmak için UseStaticFiles'ı ekleyelim
app.UseStaticFiles();

// Configure the HTTP request pipeline.
// Swagger her ortamda etkinleştirildi (development veya production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Catalog API v1");
    c.RoutePrefix = string.Empty; // Ana sayfa olarak Swagger'i ayarla
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DefaultModelsExpandDepth(0); // Modelleri gizler, daha temiz bir arayüz için
    c.InjectStylesheet("/css/swagger-custom.css"); // Özel CSS dosyamız
});

// CORS'u etkinleştirelim
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Register global exception handling middleware - use the one from SharedKernel
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use standardized API responses
app.UseStandardizedResponses();

app.UseAuthorization();

app.MapControllers();

// Seed method - kaldırıldı, artık SeedProductsToRedis sınıfı kullanılıyor
/*
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    // Örnek ürünler ekleyelim
    if (!dbContext.Products.Any())
    {
        dbContext.Products.Add(new ProductCatalog.Core.Entities.Product
        ...
    }
}
*/

// Veri ekleme işlemini sağlayalım
using (var scope = app.Services.CreateScope())
{
    var seedHelper = scope.ServiceProvider.GetRequiredService<SeedProductsToRedis>();
    await seedHelper.SeedProductsAsync();
    app.Logger.LogInformation("Örnek ürün verileri Redis'e eklendi");
}

// Log startup
app.Logger.LogInformation("Starting Product Catalog API with CQRS support and standardized response handling");

app.Run();
