using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverter.Api.Services
{
    public class CachingCurrencyService : ICurrencyService
    {
        private readonly ICurrencyService _innerService;
        private readonly IMemoryCache _memoryCache;

        public CachingCurrencyService(ICurrencyService innerService, IMemoryCache memoryCache)
        {
            _innerService = innerService;
            _memoryCache = memoryCache;
        }

        public async Task<IDictionary<string, decimal>> GetLatestAsync(string baseCurrency)
        {
            var cacheKey = $"latest_{baseCurrency}";
            if (_memoryCache.TryGetValue(cacheKey, out IDictionary<string, decimal>? rates) && rates is not null)
            {
                return rates;
            }

            var result = await _innerService.GetLatestAsync(baseCurrency);
            _memoryCache.Set(cacheKey, result, System.TimeSpan.FromMinutes(10));
            return result;
        }

        public Task<decimal> ConvertAsync(string from, string to, decimal amount)
        {
            return _innerService.ConvertAsync(from, to, amount);
        }

        public Task<(int total, IEnumerable<object> items)> GetHistoricalAsync(string baseCurrency, string from, string to, int page, int pageSize)
        {
            return _innerService.GetHistoricalAsync(baseCurrency, from, to, page, pageSize);
        }
    }
}
