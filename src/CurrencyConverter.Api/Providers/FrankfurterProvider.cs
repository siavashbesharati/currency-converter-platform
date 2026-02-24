using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CurrencyConverter.Api.Providers
{
    public class FrankfurterProvider : IExchangeRateProvider
    {
        private readonly HttpClient _http;

        public FrankfurterProvider(HttpClient http)
        {
            _http = http;
        }

        public async Task<IDictionary<string, decimal>> GetLatestRatesAsync(string baseCurrency)
        {
            var url = $"https://api.frankfurter.app/latest?base={baseCurrency}";
            var resp = await _http.GetFromJsonAsync<FrankfurterLatestResponse>(url);
            return resp?.Rates ?? new Dictionary<string, decimal>();
        }

        public async Task<IDictionary<string, IDictionary<string, decimal>>> GetHistoricalRatesAsync(string baseCurrency, string from, string to)
        {
            var url = $"https://api.frankfurter.app/{from}..{to}?base={baseCurrency}";
            var resp = await _http.GetFromJsonAsync<FrankfurterHistoricalResponse>(url);
            var result = new Dictionary<string, IDictionary<string, decimal>>();
            if (resp?.Rates != null)
            {
                foreach (var kv in resp.Rates)
                {
                    result[kv.Key] = kv.Value;
                }
            }

            return result;
        }

        private class FrankfurterLatestResponse
        {
            public string? Base { get; set; }
            public IDictionary<string, decimal>? Rates { get; set; }
            public string? Date { get; set; }
        }

        private class FrankfurterHistoricalResponse
        {
            public IDictionary<string, IDictionary<string, decimal>>? Rates { get; set; }
        }
    }
}
