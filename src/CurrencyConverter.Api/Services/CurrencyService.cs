using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyConverter.Api.Providers;

namespace CurrencyConverter.Api.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IExchangeRateProvider _provider;

        public CurrencyService(IExchangeRateProvider provider)
        {
            _provider = provider;
        }

        public async Task<IDictionary<string, decimal>> GetLatestAsync(string baseCurrency)
        {
            return await _provider.GetLatestRatesAsync(baseCurrency);
        }

        public async Task<decimal> ConvertAsync(string from, string to, decimal amount)
        {
            var rates = await GetLatestAsync(from);
            if (!rates.TryGetValue(to, out var rate))
                return 0m;
            return amount * rate;
        }

        public async Task<(int total, IEnumerable<object> items)> GetHistoricalAsync(string baseCurrency, string from, string to, int page, int pageSize)
        {
            var all = await _provider.GetHistoricalRatesAsync(baseCurrency, from, to);
            var items = all.Skip((page - 1) * pageSize).Take(pageSize).Select(kv => new { date = kv.Key, rates = kv.Value });
            return (all.Count, items);
        }
    }
}
