using System.Net;
using System.Text.Json;

namespace KanbanApp.API.Middleware
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteErrorResponse(context, ex);
            }
        }

        private static async Task WriteErrorResponse(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
                KeyNotFoundException        => (int)HttpStatusCode.NotFound,
                InvalidOperationException   => (int)HttpStatusCode.Conflict,
                ArgumentException           => (int)HttpStatusCode.BadRequest,
                _                           => (int)HttpStatusCode.InternalServerError
            };

            var body = JsonSerializer.Serialize(new
            {
                status  = context.Response.StatusCode,
                message = context.Response.StatusCode == 500
                    ? "An unexpected error occurred."
                    : ex.Message
            });

            await context.Response.WriteAsync(body);
        }
    }
}
