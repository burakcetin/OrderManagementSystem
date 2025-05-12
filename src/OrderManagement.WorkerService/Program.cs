using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Core.Behaviors;
using OrderManagement.Core.Commands.CreateOrder;
using OrderManagement.Core.Interfaces;
using OrderManagement.Core.Messages;
using OrderManagement.Core.Services;
using OrderManagement.Infrastructure.Services;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Logging;
using OrderManagement.Infrastructure.Repositories;
using OrderManagement.WorkerService;
using OrderManagement.WorkerService.Handlers;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Rebus.Bus;
 
using OrderManagement.Core.Sagas;
using OrderManagement.Core.Sagas.OrderSaga;
using SharedKernel.Data.Redis;
using OrderManagement.Core.Entities;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Configure Loki logging
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole(); // Still keep console for local debugging
            logging.AddLoki(options => 
            {
                options.LokiUrl = hostContext.Configuration["Logging:Loki:Url"] ?? "http://loki:3100";
                options.DefaultLabels.Add("Application", "order-worker");
                options.DefaultLabels.Add("Environment", hostContext.HostingEnvironment.EnvironmentName);
            });
        });
        services.AddRedisServices(hostContext.Configuration);
        services.AddRedisRepository<Order>("order");


        // Register repositories
        services.AddScoped<IOrderRepository, RedisOrderRepository>();

        // Register services
        services.AddSingleton<IEmailService, MockEmailService>();
        services.AddScoped<IProductService, ProductService>();
        
        // Register SAGA infrastructure
        services.AddScoped<ISagaCoordinator, SagaCoordinator>();
        services.AddScoped<IOrderSagaOrchestrator, OrderSagaOrchestrator>();

        // Configure Rebus with RabbitMQ
        var rabbitMQConnectionString = $"amqp://guest:guest@{hostContext.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672";

        services.AddRebus(configure => configure
            .Transport(t => t.UseRabbitMq(rabbitMQConnectionString, "order-worker-input"))
            .Routing(r => r.TypeBased()
                .Map<OrderCreatedMessage>("order-worker-input"))
            .Logging(l => l.ColoredConsole())
        );

        // Register Rebus handlers
        services.AutoRegisterHandlersFromAssemblyOf<OrderCreatedMessageHandler>();

        // Register MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Register validators
        services.AddValidatorsFromAssembly(typeof(CreateOrderCommandValidator).Assembly);

        // Register HTTP client for communicating with other microservices
        services.AddHttpClient("ProductCatalogApi", client =>
        {
            client.BaseAddress = new Uri(hostContext.Configuration["Services:ProductCatalog"] ?? "http://product-catalog-api");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
 
    })
    .Build();

// Log startup
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting OrderManagement Worker Service with SAGA Pattern and Rebus integration");

// Activate Rebus
host.Services.GetRequiredService<IBus>();
logger.LogInformation("Rebus activated for order processing");

await host.RunAsync();
