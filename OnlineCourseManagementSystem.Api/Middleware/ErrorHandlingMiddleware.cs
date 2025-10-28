using System.Net;

namespace OnlineCourseManagementSystem.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorHandlingMiddleware> logger;

        public ErrorHandlingMiddleware(RequestDelegate _next, ILogger<ErrorHandlingMiddleware> _logger)
        {
            next = _next;
            logger = _logger;
        }

        public async Task Handle(HttpContext context) 
        {
            try
            {
               await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    Message = "An unexpected error occurred. Please try again later."
                };

                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(errorJson);
            }
        }
    }
}
