using System.Collections.Generic;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Providers
{
    public interface IExchangeRateProvider
    {
        Task<IDictionary<string, decimal>> GetLatestRatesAsync(string baseCurrency);

        Task<IDictionary<string, IDictionary<string, decimal>>> GetHistoricalRatesAsync(string baseCurrency, string from, string to);
    }
}
