using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Api
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register application services, e.g. ICurrencyService
            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register providers, DbContext, caching, HttpClient with Polly
            return services;
        }

        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            // Configure JWT authentication and RBAC
            return services;
        }
    }
}
