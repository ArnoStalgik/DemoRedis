using DemoRedis.Models;
using System.Net;

namespace DemoRedis.Services
{
    public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new ApiResponse
                {
                    Success = false,
                    Error = ex.Message
                }));
            }
        }
    }
}