using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Api.Providers
{
    public static class ExchangeRateProviderFactory
    {
        public static IExchangeRateProvider Create(IServiceProvider sp)
        {
            // For now, return FrankfurterProvider from DI container
            return sp.GetRequiredService<FrankfurterProvider>();
        }
    }
}
