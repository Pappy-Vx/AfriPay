using AfriPay.API.Extensions;
using AfriPay.API.Infrastructure;
using AfriPay.APP;
using AfriPay.APP.EventHandlers;
using AfriPay.APP.Services;
using AfriPay.CORE.Interfaces;
using AfriPay.DAL.Data;
using AfriPay.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/afripay-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq(serverUrl: "http://localhost:5341", apiKey: null)
    .CreateLogger();

try
{
    Log.Information("Starting AfriPay API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add controllers with filters
    builder.Services.AddControllers();

    // Add API versioning
    builder.Services.AddApiVersioningConfiguration();

    // Add Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "AfriPay API", Version = "v1.0" });
        c.EnableAnnotations();
    });

    //---------------------------------------------
    // Configure Database Context
    //---------------------------------------------
    builder.Services.AddDbContext<AfriPayDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Register IApplicationDbContext
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<AfriPayDbContext>());

    // Register Repositories
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IAccountRepository, AccountRepository>();
    builder.Services.AddScoped<IOnboardingRequestRepository, OnboardingRequestRepository>();

    // Repository and Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Add Application Layer Services (MediatR, FluentValidation, Behaviors)
    builder.Services.AddApplicationServices();

    // Application Services (legacy - will be replaced by MediatR)
    builder.Services.AddScoped<IOnboardingService, OnboardingService>();

    // Event Handlers
    builder.Services.AddScoped<OnboardingRequestedHandler>();
    builder.Services.AddScoped<BvnVerifiedForOnboardingHandler>();
    builder.Services.AddScoped<CustomerCreatedHandler>();

    // External Service Implementations (Mock for now)
    builder.Services.AddScoped<IBvnVerificationService, MockBvnVerificationService>();
    builder.Services.AddScoped<IVirtualAccountProvider, MockVirtualAccountProvider>();
    builder.Services.AddScoped<IEventPublisher, InMemoryEventPublisher>();

    // CORS
    builder.Services.AddCorsPolicy("AllowAll");

    var app = builder.Build();

    // Configure the HTTP request pipeline.

    // Use custom middleware (logging and exception handling)
    app.UseCustomMiddleware();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AfriPay API v1.0");
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("AfriPay API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AfriPay API failed to start");
}
finally
{
    Log.CloseAndFlush();
}
