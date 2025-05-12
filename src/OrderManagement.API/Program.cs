using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MediatR;
using OrderManagement.Core.Behaviors; 
using OrderManagement.Core.Interfaces;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Logging;
using OrderManagement.Infrastructure.Repositories;
using ProductCatalog.Core.Interfaces;
 
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharedKernel.Api;
using SharedKernel.Data.Redis;
using OrderManagement.Core.Messages;
using ProductCatalog.Infrastructure.Repositories;
using ProductCatalog.Infrastructure.Data;
using OrderManagement.Core.Services;
using OrderManagement.Infrastructure.Services;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Rebus.Bus;
using OrderManagement.Core.Commands.SagaCommands;
 
using OrderManagement.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// Configure Loki logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();  
builder.Logging.AddLoki(options => 
{
    options.LokiUrl = builder.Configuration["Logging:Loki:Url"] ?? "http://loki:3100";
    options.DefaultLabels.Add("Application", "order-api");
    options.DefaultLabels.Add("Environment", builder.Environment.EnvironmentName);
}); 
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
 
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        
      
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        
 
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        
        
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
        Title = "Order Management API",
        Version = "v1",
        Description = "API for managing orders in e-commerce platform",
        Contact = new OpenApiContact
        {
            Name = "DigiCase",
            Email = "info@example.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Redis services
builder.Services.AddRedisServices(builder.Configuration);
builder.Services.AddRedisRepository<Order>("order");

 
// builder.Services.AddScoped<IOrderRepository, OrderRepository>(); // InMemory repository - deaktif
builder.Services.AddScoped<IOrderRepository, RedisOrderRepository>(); // Redis repository

// Register services
builder.Services.AddSingleton<IEmailService, MockEmailService>();
builder.Services.AddScoped<IProductService, ProductService>();
 
// Configure Rebus with RabbitMQ
var rabbitMQConnectionString = $"amqp://guest:guest@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672";
builder.Services.AddRebus(configure => configure
   .Transport(t => t.UseRabbitMq(rabbitMQConnectionString, "order-api-input"))
   .Routing(r => r.TypeBased()
       .Map<OrderCreatedMessage>("order-worker-input"))
   .Logging(l => l.ColoredConsole())
);

 
builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();

 
 
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(ProcessPaymentCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

 
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

 
builder.Services.AddHttpClient("ProductCatalogApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductCatalog"] ?? "http://product-catalog-api");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.UseStaticFiles();

 
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1");
    c.RoutePrefix = string.Empty;  
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DefaultModelsExpandDepth(0);  
    c.InjectStylesheet("/css/swagger-custom.css");  
});

app.UseHttpsRedirection();

 
app.UseMiddleware<ExceptionHandlingMiddleware>();
 
app.UseStandardizedResponses();

app.UseAuthorization();

app.MapControllers();
 
// Seed data sonrası Rebus'u aktif et
app.Services.GetRequiredService<IBus>();
app.Logger.LogInformation("Rebus aktif edildi");

// Log startup
app.Logger.LogInformation("Starting OrderManagement API with Rebus integration and standardized response handling");

 
app.Run();
