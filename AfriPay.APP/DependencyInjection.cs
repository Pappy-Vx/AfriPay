using AfriPay.APP.Common.Behaviours;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AfriPay.APP
{
    /// <summary>
    /// Dependency injection configuration for the Application layer
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);

                // Register pipeline behaviors (order matters - they execute in order of registration)
                cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
                cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            });

            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
