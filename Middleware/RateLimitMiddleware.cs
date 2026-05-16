using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Middleware
{
    public class RateLimitMiddleware
    {
        private static readonly ConcurrentDictionary<string, int> RequestCounts = new();
        private readonly RequestDelegate _next;

        public RateLimitMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            if (clientIp != null)
            {
                RequestCounts.AddOrUpdate(clientIp, 1, (_, count) => count + 1);

                if (RequestCounts[clientIp] > 100)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Rate limit exceeded.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
