using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;

namespace CurrencyConverter.Api.Providers
{
    public class FrankfurterProvider : IExchangeRateProvider
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FrankfurterProvider(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IDictionary<string, decimal>> GetLatestRatesAsync(string baseCurrency)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"latest?base={baseCurrency}");
            var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            if(correlationId is not null)
            {
                request.Headers.Add("X-Correlation-ID", correlationId);
            }
            var resp = await _http.SendAsync(request);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadFromJsonAsync<FrankfurterLatestResponse>();
            return content?.Rates ?? new Dictionary<string, decimal>();
        }

        public async Task<IDictionary<string, IDictionary<string, decimal>>> GetHistoricalRatesAsync(string baseCurrency, string from, string to)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{from}..{to}?base={baseCurrency}");
            var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            if(correlationId is not null)
            {
                request.Headers.Add("X-Correlation-ID", correlationId);
            }
            var resp = await _http.SendAsync(request);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadFromJsonAsync<FrankfurterHistoricalResponse>();
            var result = new Dictionary<string, IDictionary<string, decimal>>();
            if (content?.Rates != null)
            {
                foreach (var kv in content.Rates)
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
