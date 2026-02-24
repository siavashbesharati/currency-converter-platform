using Xunit;
using Moq;
using CurrencyConverter.Api.Services;
using CurrencyConverter.Api.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Tests
{
    public class CurrencyServiceTests
    {
        private readonly Mock<IExchangeRateProvider> _mockProvider;
        private readonly CurrencyService _service;

        public CurrencyServiceTests()
        {
            _mockProvider = new Mock<IExchangeRateProvider>();
            _service = new CurrencyService(_mockProvider.Object);
        }

        [Fact]
        public async Task GetLatestAsync_Returns_Rates_From_Provider()
        {
            // Arrange
            var baseCurrency = "USD";
            var expectedRates = new Dictionary<string, decimal> { { "EUR", 0.85m } };
            _mockProvider.Setup(p => p.GetLatestRatesAsync(baseCurrency)).ReturnsAsync(expectedRates);

            // Act
            var result = await _service.GetLatestAsync(baseCurrency);

            // Assert
            Assert.Equal(expectedRates, result);
        }

        [Fact]
        public async Task ConvertAsync_Returns_Correct_Conversion()
        {
            // Arrange
            var from = "USD";
            var to = "EUR";
            var amount = 100;
            var rates = new Dictionary<string, decimal> { { "EUR", 0.85m } };
            _mockProvider.Setup(p => p.GetLatestRatesAsync(from)).ReturnsAsync(rates);
            var expectedConversion = 85;

            // Act
            var result = await _service.ConvertAsync(from, to, amount);

            // Assert
            Assert.Equal(expectedConversion, result);
        }

        [Fact]
        public async Task GetHistoricalAsync_Returns_Paginated_Data()
        {
            // Arrange
            var baseCurrency = "USD";
            var from = "2023-01-01";
            var to = "2023-01-02";
            var page = 1;
            var pageSize = 1;
            var historicalData = new Dictionary<string, IDictionary<string, decimal>>
            {
                { "2023-01-01", new Dictionary<string, decimal> { { "EUR", 0.85m } } },
                { "2023-01-02", new Dictionary<string, decimal> { { "EUR", 0.86m } } }
            };
            _mockProvider.Setup(p => p.GetHistoricalRatesAsync(baseCurrency, from, to)).ReturnsAsync(historicalData);

            // Act
            var (total, items) = await _service.GetHistoricalAsync(baseCurrency, from, to, page, pageSize);

            // Assert
            Assert.Equal(2, total);
            var itemsList = new List<object>(items);
            Assert.Single(itemsList);
        }
    }
}
