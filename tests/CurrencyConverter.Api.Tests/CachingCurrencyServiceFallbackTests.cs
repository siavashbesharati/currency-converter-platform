using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Polly.CircuitBreaker;
using Xunit;

using CurrencyConverter.Api.Services;
using CurrencyConverter.Api.Data;

namespace CurrencyConverter.Api.Tests
{
    public class CachingCurrencyServiceFallbackTests
    {
        [Fact]
        public async Task GetLatestAsync_Returns_DbCache_When_CircuitIsOpen()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ExchangeRateDbContext>()
                .UseInMemoryDatabase(databaseName: "FallbackTestDb")
                .Options;

            var memCache = new MemoryCache(new MemoryCacheOptions());

            using (var db = new ExchangeRateDbContext(options))
            {
                db.ExchangeRateCaches.Add(new ExchangeRateCache
                {
                    BaseCurrency = "USD",
                    RatesJson = JsonSerializer.Serialize(new Dictionary<string, decimal> { { "EUR", 0.9123m } }),
                    Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
                db.SaveChanges();

                var mockInner = new Mock<ICurrencyService>();
                mockInner.Setup(s => s.GetLatestAsync("USD")).ThrowsAsync(new BrokenCircuitException("circuit open"));

                var caching = new CachingCurrencyService(mockInner.Object, memCache, db);

                // Act
                var result = await caching.GetLatestAsync("USD");

                // Assert
                Assert.NotNull(result);
                Assert.True(result.ContainsKey("EUR"));
                Assert.Equal(0.9123m, result["EUR"]);
            }
        }
    }
}
