using System.Collections.Generic;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Services
{
    public interface ICurrencyService
    {
        Task<IDictionary<string, decimal>> GetLatestAsync(string baseCurrency);

        Task<decimal> ConvertAsync(string from, string to, decimal amount);

        Task<(int total, IEnumerable<object> items)> GetHistoricalAsync(string baseCurrency, string from, string to, int page, int pageSize);
    }
}
