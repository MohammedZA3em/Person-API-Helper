using System.Net;
using System.Text.Json;

namespace YourNamespace.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }

            // بعد تنفيذ الـ controller، إذا كان status code >= 400
            // (أي خطأ من BadRequest أو NotFound ...)، نوحّد الشكل أيضًا
            if (context.Response.StatusCode >= 400 && !context.Response.HasStarted)
            {
                // هنا يمكن أن تكون الرسالة قد تم إرجاعها مسبقًا كنص أو object
                // سنقرأها ونحوّلها إلى Array統 واحد.
                context.Response.ContentType = "application/json";
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var result = JsonSerializer.Serialize(new[]
            {
                "Database connection failed!"
            });

            return context.Response.WriteAsync(result);
        }
    }
}
