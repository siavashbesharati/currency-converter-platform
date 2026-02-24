using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CurrencyConverter.Api.Providers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CurrencyConverter.Api.Tests
{
    public class CurrencyControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public CurrencyControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetLatest_Returns_Ok()
        {
            // Arrange
            var mockProvider = new Mock<IExchangeRateProvider>();
            mockProvider.Setup(p => p.GetLatestRatesAsync("USD")).ReturnsAsync(new Dictionary<string, decimal> { { "EUR", 0.85m } });

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IExchangeRateProvider>(_ => mockProvider.Object);
                    // Use the real CurrencyService (no caching wrapper) for controller tests to avoid DB dependencies
                    services.AddScoped<CurrencyConverter.Api.Services.CurrencyService>();
                    services.AddScoped<CurrencyConverter.Api.Services.ICurrencyService, CurrencyConverter.Api.Services.CurrencyService>();
                    services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "Test"; options.DefaultChallengeScheme = "Test"; })
                        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/currency/latest?baseCurrency=USD");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<object>();
            Assert.NotNull(content);
        }

        [Fact]
        public async Task Convert_Returns_Ok()
        {
            // Arrange
            var mockProvider = new Mock<IExchangeRateProvider>();
            mockProvider.Setup(p => p.GetLatestRatesAsync("USD")).ReturnsAsync(new Dictionary<string, decimal> { { "EUR", 0.85m } });

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IExchangeRateProvider>(_ => mockProvider.Object);
                    services.AddScoped<CurrencyConverter.Api.Services.CurrencyService>();
                    services.AddScoped<CurrencyConverter.Api.Services.ICurrencyService, CurrencyConverter.Api.Services.CurrencyService>();
                    services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "Test"; options.DefaultChallengeScheme = "Test"; })
                        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            }).CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/v1/currency/convert", new { amount = 100, source = "USD", target = "EUR" });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<object>();
            Assert.NotNull(content);
        }
    }
}
