using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Rentora.Core.Models;
using Rentora.Infrastructure.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rentora.API.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            try
            {
                // Let the API run normally
                await _next(context);
            }
            catch (Exception ex)
            {
                // The net caught an error! Log it to the console
                _logger.LogError(ex, "An unhandled exception occurred.");

                // 1. Save the exact error details to your SQL Database
                var errorLog = new RErrorLog
                {
                    Timestamp = DateTime.Now,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                    RequestUrl = $"{context.Request.Method} {context.Request.Path}"
                };

                dbContext.RErrorLogs.Add(errorLog);
                await dbContext.SaveChangesAsync();

                // 2. Send a clean, polite JSON response to the Ionic-Vue mobile app
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var response = new { Message = "An unexpected error occurred. Our team has been notified." };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class GlobalExceiptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceiptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
