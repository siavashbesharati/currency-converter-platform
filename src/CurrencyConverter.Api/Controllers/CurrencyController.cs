using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.Api.Services;

namespace CurrencyConverter.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private static readonly string[] Excluded = new[] { "TRY", "PLN", "THB", "MXN" };
        private readonly ICurrencyService _service;

        public CurrencyController(ICurrencyService service)
        {
            _service = service;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest([FromQuery][Required] string baseCurrency)
        {
            if (IsExcluded(baseCurrency))
                return BadRequest(new { error = "Base currency is excluded: " + baseCurrency });
            var rates = await _service.GetLatestAsync(baseCurrency);
            return Ok(new { baseCurrency, rates });
        }

        [HttpPost("convert")]
        public async Task<IActionResult> Convert([FromBody] CurrencyConversionRequest req)
        {
            if (IsExcluded(req.Source) || IsExcluded(req.Target))
                return BadRequest(new { error = "One of the currencies is excluded (TRY, PLN, THB, MXN)." });
            var converted = await _service.ConvertAsync(req.Source, req.Target, req.Amount);
            return Ok(new CurrencyConversionResponse { Source = req.Source, Target = req.Target, Amount = req.Amount, Converted = converted });
        }

        [HttpGet("historical")]
        public async Task<IActionResult> Historical([FromQuery][Required] string baseCurrency, [FromQuery][Required] string from, [FromQuery][Required] string to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (IsExcluded(baseCurrency))
                return BadRequest(new { error = "Base currency is excluded: " + baseCurrency });
            var (total, items) = await _service.GetHistoricalAsync(baseCurrency, from, to, page, pageSize);
            return Ok(new { baseCurrency, from, to, page, pageSize, total, items });
        }

        private static bool IsExcluded(string? currency)
        {
            if (string.IsNullOrWhiteSpace(currency)) return false;
            return Excluded.Contains(currency.ToUpperInvariant());
        }
    }

    public record CurrencyConversionRequest
    {
        public decimal Amount { get; init; }
        public string Source { get; init; } = string.Empty;
        public string Target { get; init; } = string.Empty;
    }

    public record CurrencyConversionResponse
    {
        public decimal Amount { get; init; }
        public decimal Converted { get; init; }
        public string Source { get; init; } = string.Empty;
        public string Target { get; init; } = string.Empty;
    }
}
