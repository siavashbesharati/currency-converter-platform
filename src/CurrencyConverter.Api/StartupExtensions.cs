using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using CurrencyConverter.Api.Data;
using CurrencyConverter.Api.Services;
using CurrencyConverter.Api.Providers;
using CurrencyConverter.Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CurrencyService>();
            services.AddScoped<ICurrencyService, CachingCurrencyService>(s => 
                new CachingCurrencyService(s.GetRequiredService<CurrencyService>(), s.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(), s.GetRequiredService<ExchangeRateDbContext>()));
            services.AddSingleton<TokenService>();

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            // DbContext (SQLite by default)
            var conn = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=currency.db";
            services.AddDbContext<ExchangeRateDbContext>(opts => opts.UseSqlite(conn));

            // Register Frankfurter provider HttpClient with Polly resilience
            static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
            {
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(new[] {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4)
                    });
            }

            static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
            {
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));
            }

            services.AddHttpClient<FrankfurterProvider>(client =>
            {
                client.BaseAddress = new Uri("https://api.frankfurter.app/");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddScoped<IExchangeRateProvider>(sp => sp.GetRequiredService<FrankfurterProvider>());

            return services;
        }

        public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("JwtSettings");
            var jwt = jwtSection.Get<Auth.JwtSettings>() ?? new Auth.JwtSettings();
            var secret = jwt.Secret ?? "REPLACE_THIS_WITH_STRONG_KEY";
            var key = Encoding.UTF8.GetBytes(secret);

            services.Configure<Auth.JwtSettings>(jwtSection);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
