using AfriPay.API.Infrastructure;
using AfriPay.APP.EventHandlers;
using AfriPay.APP.Services;
using AfriPay.CORE.Interfaces;
using AfriPay.DAL.Data;
using AfriPay.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AfriPay API", Version = "v1" });
});

//---------------------------------------------
// 2 Configure Database Context
//---------------------------------------------

builder.Services.AddDbContext<AfriPayDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Repository and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
