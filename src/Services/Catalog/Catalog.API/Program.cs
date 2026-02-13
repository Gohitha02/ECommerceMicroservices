using Carter;
using Catalog.API.Data;
using Catalog.API.Models;
using EventBus.Abstractions;
using EventBus.Implementations;
using FluentValidation;
using Marten;
using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console()
          .Enrich.FromLogContext();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCarter();

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("CatalogConnection")!);
    options.Schema.For<Product>().Index(x => x.Category);
}).UseLightweightSessions();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

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
    return new RabbitMQEventBus(rabbitMQPersistentConnection, logger, serviceProvider, eventBusSubcriptionsManager, "Catalog", 5);
});

builder.Services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapCarter();

using (var scope = app.Services.CreateScope())
{
    var documentStore = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
    await CatalogDataSeeder.SeedAsync(documentStore);
}

app.Run();