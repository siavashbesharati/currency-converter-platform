namespace CurrencyConverter.Api.Services
{
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using CurrencyConverter.Api.Data;
using Microsoft.EntityFrameworkCore;

    public class CachingCurrencyService : ICurrencyService
    {
        private readonly ICurrencyService _innerService;
        private readonly IMemoryCache _memoryCache;
        private readonly ExchangeRateDbContext _dbContext;

        public CachingCurrencyService(ICurrencyService innerService, IMemoryCache memoryCache, ExchangeRateDbContext dbContext)
        {
            _innerService = innerService;
            _memoryCache = memoryCache;
            _dbContext = dbContext;
        }

        public async Task<IDictionary<string, decimal>> GetLatestAsync(string baseCurrency)
        {
            var cacheKey = $"latest_{baseCurrency}";
            if (_memoryCache.TryGetValue(cacheKey, out IDictionary<string, decimal>? rates) && rates is not null)
            {
                return rates;
            }

            if(_dbContext.ExchangeRateCaches is null)
            {
                // Should not happen, but satisfies nullable check
                return await _innerService.GetLatestAsync(baseCurrency);
            }
            var dbCache = await _dbContext.ExchangeRateCaches.FirstOrDefaultAsync(c => c.BaseCurrency == baseCurrency);
            if (dbCache is not null && !string.IsNullOrEmpty(dbCache.RatesJson))
            {
                var cachedRates = JsonSerializer.Deserialize<IDictionary<string, decimal>>(dbCache.RatesJson);
                if (cachedRates is not null)
                {
                    _memoryCache.Set(cacheKey, cachedRates, System.TimeSpan.FromMinutes(10));
                    return cachedRates;
                }
            }

            IDictionary<string, decimal> result;
            try
            {
                result = await _innerService.GetLatestAsync(baseCurrency);
            }
            catch (Polly.CircuitBreaker.BrokenCircuitException)
            {
                // Circuit is open - try to return DB cached value if present
                if (dbCache is not null && !string.IsNullOrEmpty(dbCache.RatesJson))
                {
                    var cachedRates = JsonSerializer.Deserialize<IDictionary<string, decimal>>(dbCache.RatesJson);
                    if (cachedRates is not null)
                    {
                        _memoryCache.Set(cacheKey, cachedRates, System.TimeSpan.FromMinutes(10));
                        return cachedRates;
                    }
                }

                // No cached DB value - rethrow to surface error
                throw;
            }

            var newDbCache = new ExchangeRateCache
            {
                BaseCurrency = baseCurrency,
                RatesJson = JsonSerializer.Serialize(result),
                Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            if(_dbContext.ExchangeRateCaches is not null)
            {
                _dbContext.ExchangeRateCaches.Add(newDbCache);
                await _dbContext.SaveChangesAsync();
            }

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
