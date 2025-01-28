using System.Reflection;
using CarRental.API.Controllers;
using CarRental.API.Middlewares;
using CarRental.Application.Requests;
using CarRental.Application.Validators;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using CarRental.Domain;
using CarRental.Domain.Interfaces;
using CarRental.Domain.PriceCalculationStrategies;
using CarRental.Infrastructure.Caching;
using CarRental.Infrastructure.DbContext;
using CarRental.Infrastructure.Interfaces;
using CarRental.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add controllers without auto-validation
builder.Services.AddControllers();

// Manually configure FluentValidation
builder.Services.AddScoped<IValidator<RentalPickupRequest>, RentalPickupRequestValidator>();
builder.Services.AddScoped<IValidator<ReturnRequest>, ReturnRequestValidator>();
builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true;
});

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<RentalPickupRequestValidator>();

var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionStrings") 
                            ?? "localhost:6379";

// Configure Redis with more resilient settings
var redisConfig = new ConfigurationOptions
{
    EndPoints = { redisConnectionString },
    ConnectTimeout = 10000, // 10 seconds
    SyncTimeout = 10000,
    AbortOnConnectFail = false,
    ConnectRetry = 3,
    AllowAdmin = true
};

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConfig));

// Then register your Redis cache service
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// Register MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.Load("CarRental.Application"));
});

// Add DbContext
builder.Services.AddDbContext<CarRentalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Add Application Layer
builder.Services.AddScoped<IPriceCalculationStrategy, SmallCarPriceCalculation>(); // Default strategy

// Add Repository
builder.Services.AddScoped<IRentalRepository, RentalRepository>();

// Add Price Calculation Strategy Factory
builder.Services.AddScoped<IPriceCalculationStrategyFactory, PriceCalculationStrategyFactory>();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Car rental",
        Version = "v1",
        Description = "A car rental API"
    });
});

var app = builder.Build();

// Configure Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Car rental V1");
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();