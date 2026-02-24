using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace CurrencyConverter.Api.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrSetCorrelationId(context);
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                {
                    context.Response.Headers[CorrelationIdHeaderName] = correlationId;
                }
                return Task.CompletedTask;
            });
            await _next(context);
        }

        private static string GetOrSetCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationIdValues) && correlationIdValues.Any())
            {
                var correlationId = correlationIdValues.First();
                if(!string.IsNullOrEmpty(correlationId))
                {
                    context.TraceIdentifier = correlationId;
                    return correlationId;
                }
            }

            var newCorrelationId = Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationIdHeaderName] = newCorrelationId;
            context.TraceIdentifier = newCorrelationId;
            return newCorrelationId;
        }
    }
}
