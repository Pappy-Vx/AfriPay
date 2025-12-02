using AfriPay.API.Extensions;
using AfriPay.API.Infrastructure;
using AfriPay.API.Middlewares;
using AfriPay.APP;
using AfriPay.APP.EventHandlers;
using AfriPay.APP.Services;
using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using AfriPay.DAL.BackgroundJobs;
using AfriPay.DAL.Data;
using AfriPay.DAL.Repositories;
using AfriPay.DAL.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

// =====================================================================
// SERILOG CONFIGURATION
// =====================================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "AfriPay.API")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/afripay-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting AfriPay API");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // =====================================================================
    // DATABASE
    // =====================================================================
    builder.Services.AddDbContext<AfriPayDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

    // =====================================================================
    // CORE & INFRASTRUCTURE
    // =====================================================================
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<AfriPayDbContext>());

    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IAccountRepository, AccountRepository>();
    builder.Services.AddScoped<IOnboardingRequestRepository, OnboardingRequestRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

    // =====================================================================
    // EXTERNAL SERVICES (MOCKS FOR DEVELOPMENT)
    // =====================================================================
    builder.Services.AddScoped<IBvnVerificationService, MockBvnVerificationService>();
    builder.Services.AddScoped<IVirtualAccountProvider, MockVirtualAccountProvider>();
    builder.Services.AddScoped<IEventPublisher, InMemoryEventPublisher>();

    // =====================================================================
    // DOMAIN EVENT HANDLERS
    // =====================================================================
    builder.Services.AddTransient<INotificationHandler<OnboardingRequestedEvent>, OnboardingRequestedHandler>();
    builder.Services.AddTransient<INotificationHandler<BvnVerifiedForOnboardingEvent>, BvnVerifiedForOnboardingHandler>();
    builder.Services.AddTransient<INotificationHandler<CustomerCreatedEvent>, CustomerCreatedHandler>();

    // =====================================================================
    // APPLICATION SERVICES
    // =====================================================================
    builder.Services.AddApplicationServices(); // MediatR, FluentValidation, Behaviors
    builder.Services.AddScoped<IOnboardingService, OnboardingService>();

    // =====================================================================
    // BACKGROUND JOBS
    // =====================================================================
    builder.Services.AddHostedService<OnboardingWorker>();

    // =====================================================================
    // CONTROLLERS + SWAGGER
    // =====================================================================
    builder.Services.AddControllers();
    builder.Services.AddApiVersioningConfiguration();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "AfriPay API",
            Version = "v1",
            Description = "African Payments Onboarding API with Clean Architecture"
        });
    });

    // =====================================================================
    // BUILD APP
    // =====================================================================
    var app = builder.Build();

    // =====================================================================
    // MIDDLEWARE PIPELINE
    // =====================================================================
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AfriPay API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();

    // =====================================================================
    // ROUTING
    // =====================================================================
    app.MapControllers();
    app.MapGet("/", () => "AfriPay API is running. Visit /swagger for documentation.");


    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AfriPayDbContext>();
        try
        {
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrated successfully");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database migration failed - ensure SQL Server is running");
        }
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "AfriPay API failed to start");
}
finally
{
    Log.CloseAndFlush();
}