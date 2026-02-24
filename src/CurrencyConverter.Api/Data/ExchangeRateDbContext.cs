using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Api.Data
{
    public class ExchangeRateDbContext : DbContext
    {
        public ExchangeRateDbContext(DbContextOptions<ExchangeRateDbContext> options) : base(options)
        {
        }

        public DbSet<ExchangeRateCache>? ExchangeRateCaches { get; set; }
    }

    public class ExchangeRateCache
    {
        public int Id { get; set; }
        public string? BaseCurrency { get; set; }
        public string? RatesJson { get; set; }
        public long Timestamp { get; set; }
    }
}
