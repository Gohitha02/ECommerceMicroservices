using Basket.API.Data;
using Basket.API.IntegrationEvents.EventHandling;
using Basket.API.IntegrationEvents.Events;
using Carter;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using EventBus.Abstractions;
using EventBus.Implementations;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console()
          .Enrich.FromLogContext();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCarter();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Basket_";
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddSingleton<IBasketRepository, BasketRepository>();

builder.Services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["EventBus:HostName"] ?? "localhost",
        UserName = builder.Configuration["EventBus:UserName"] ?? "guest",
        Password = builder.Configuration["EventBus:Password"] ?? "guest",
       
    };
    return new DefaultRabbitMQPersistentConnection(factory, logger);
});

builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>(sp =>
{
    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
    var serviceProvider = sp;
    return new RabbitMQEventBus(rabbitMQPersistentConnection, logger, serviceProvider, eventBusSubcriptionsManager, "Basket", 5);
});

builder.Services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
builder.Services.AddTransient<ProductPriceChangedIntegrationEventHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapCarter();

var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<ProductPriceChangedIntegrationEvent, ProductPriceChangedIntegrationEventHandler>();

app.Run();