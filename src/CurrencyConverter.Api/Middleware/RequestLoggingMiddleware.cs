using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            // Extract client IP
            var clientIp = context.Connection?.RemoteIpAddress?.ToString();

            // Extract client id (from JWT) - use Name or sub claim if present
            string? clientId = null;
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                clientId = context.User.FindFirst(ClaimTypes.Name)?.Value ?? context.User.FindFirst("sub")?.Value;
            }

            // Request info
            var method = context.Request?.Method;
            var path = context.Request?.Path;

            // Proceed
            await _next(context);

            sw.Stop();

            var status = context.Response?.StatusCode;

            _logger.LogInformation("HTTP {Method} {Path} responded {Status} in {Elapsed}ms {@Meta}",
                method, path, status, sw.ElapsedMilliseconds, new {
                    ClientIp = clientIp,
                    ClientId = clientId,
                    CorrelationId = context.TraceIdentifier
                });
        }
    }
}
