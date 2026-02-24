using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private static readonly string[] Excluded = new[] { "TRY", "PLN", "THB", "MXN" };

        [HttpGet("latest")]
        public IActionResult GetLatest([FromQuery][Required] string baseCurrency)
        {
            if (IsExcluded(baseCurrency))
                return BadRequest(new { error = "Base currency is excluded: " + baseCurrency });

            // TODO: call ICurrencyService to get latest rates
            return Ok(new { baseCurrency, rates = new { USD = 1.0 } });
        }

        [HttpPost("convert")]
        public IActionResult Convert([FromBody] CurrencyConversionRequest req)
        {
            if (IsExcluded(req.Source) || IsExcluded(req.Target))
                return BadRequest(new { error = "One of the currencies is excluded (TRY, PLN, THB, MXN)." });

            // TODO: call ICurrencyService to perform conversion
            return Ok(new CurrencyConversionResponse { Source = req.Source, Target = req.Target, Amount = req.Amount, Converted = req.Amount * 1.0m });
        }

        [HttpGet("historical")]
        public IActionResult Historical([FromQuery][Required] string baseCurrency, [FromQuery][Required] string from, [FromQuery][Required] string to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (IsExcluded(baseCurrency))
                return BadRequest(new { error = "Base currency is excluded: " + baseCurrency });

            // TODO: call ICurrencyService to fetch historical rates with pagination
            return Ok(new { baseCurrency, from, to, page, pageSize, items = new object[0] });
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
