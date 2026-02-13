using Carter;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Data;
using Ordering.API.IntegrationEvents.EventHandling;
using Ordering.API.IntegrationEvents.Events;
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

builder.Services.AddDbContext<OrderingDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderingConnection"));
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["EventBus:HostName"] ?? "localhost",
        UserName = builder.Configuration["EventBus:UserName"] ?? "guest",
        Password = builder.Configuration["EventBus:Password"] ?? "guest",
        DispatchConsumersAsync = true
    };
    return new DefaultRabbitMQPersistentConnection(factory, logger);
});

builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>(sp =>
{
    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
    var serviceProvider = sp;
    return new RabbitMQEventBus(rabbitMQPersistentConnection, logger, serviceProvider, eventBusSubcriptionsManager, "Ordering", 5);
});

builder.Services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
builder.Services.AddTransient<BasketCheckoutIntegrationEventHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapCarter();

var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<BasketCheckoutIntegrationEvent, BasketCheckoutIntegrationEventHandler>();

app.Run();