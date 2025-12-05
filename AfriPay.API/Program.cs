using AfriPay.API.Extensions;
using AfriPay.API.Hubs;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text;
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
    builder.Services.AddScoped<ITransferRepository, TransferRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
    // =====================================================================
    // EXTERNAL SERVICES (MOCKS FOR DEVELOPMENT)
    // =====================================================================
    builder.Services.AddScoped<IBvnVerificationService, MockBvnVerificationService>();
    builder.Services.AddScoped<IGhanaCardVerificationService, MockGhanaCardVerificationService>();
    builder.Services.AddScoped<IKenyaNationalIdVerificationService, MockKenyaNationalIdVerificationService>();
    builder.Services.AddScoped<IVirtualAccountProvider, MockVirtualAccountProvider>();
    builder.Services.AddScoped<IEventPublisher, InMemoryEventPublisher>();
    // =====================================================================
    // AUTHENTICATION SERVICES
    // =====================================================================
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    // =====================================================================
    // DOMAIN EVENT HANDLERS
    // =====================================================================
    builder.Services.AddTransient<INotificationHandler<OnboardingRequestedEvent>, OnboardingRequestedHandler>();
    builder.Services.AddTransient<INotificationHandler<BvnVerifiedForOnboardingEvent>, BvnVerifiedForOnboardingHandler>();
    builder.Services.AddTransient<INotificationHandler<GhanaCardVerifiedForOnboardingEvent>, GhanaCardVerifiedForOnboardingHandler>();
    builder.Services.AddTransient<INotificationHandler<KenyaIdVerifiedForOnboardingEvent>, KenyaIdVerifiedForOnboardingHandler>();
    builder.Services.AddTransient<INotificationHandler<CustomerCreatedEvent>, CustomerCreatedHandler>();
    builder.Services.AddTransient<INotificationHandler<TransferInitiatedEvent>, TransferInitiatedEventHandler>();
    builder.Services.AddTransient<INotificationHandler<TransferCompletedEvent>, TransferCompletedEventHandler>();
    builder.Services.AddTransient<INotificationHandler<TransferFailedEvent>, TransferFailedEventHandler>();
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
    // JWT AUTHENTICATION
    // =====================================================================
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("Token validated for user: {UserId}",
                    context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });
    builder.Services.AddAuthorization();
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
            Description = "African Payments Onboarding API with Clean Architecture and JWT Authentication",
            Contact = new OpenApiContact
            {
                Name = "AfriPay Support",
                Email = "support@afripay.com"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // Include XML comments from API project
        var xmlFile = "AfriPay.API.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        Log.Information("Looking for API XML file at: {XmlPath}", xmlPath);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            Log.Information("Successfully loaded API XML comments");
        }
        else
        {
            Log.Warning("API XML file not found at: {XmlPath}", xmlPath);
        }

        // Include XML comments from APP project (for DTOs and Commands)
        var appXmlFile = "AfriPay.APP.xml";
        var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
        Log.Information("Looking for APP XML file at: {AppXmlPath}", appXmlPath);
        if (File.Exists(appXmlPath))
        {
            c.IncludeXmlComments(appXmlPath, includeControllerXmlComments: false);
            Log.Information("Successfully loaded APP XML comments");
        }
        else
        {
            Log.Warning("APP XML file not found at: {AppXmlPath}", appXmlPath);
        }

        c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");

        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme.
                          Enter 'Bearer' [space] and then your token in the text input below.
                          Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });

        // Enable annotations for Swagger attributes like [SwaggerOperation], [SwaggerResponse]
        c.EnableAnnotations();

        // Use full schema IDs to avoid conflicts
        c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

        // Map CustomerId to Guid for Swagger
        c.MapType<AfriPay.CORE.ValueObjects.CustomerId>(() => new Microsoft.OpenApi.Models.OpenApiSchema
        {
            Type = "string",
            Format = "uuid",
            Example = new Microsoft.OpenApi.Any.OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6")
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
            c.DisplayRequestDuration();
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            c.EnableDeepLinking();
            c.EnableFilter();
        });
    }
    app.UseHttpsRedirection();
    // Authentication must come before Authorization
    app.UseAuthentication();
    app.UseAuthorization();
    // =====================================================================
    // ROUTING
    // =====================================================================
    app.MapControllers();
    app.MapGet("/", () => "AfriPay API is running. Visit /swagger for documentation.");
    // Map SignalR hub
    //app.MapHub<AccountBalanceHub>("/hubs/account");
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