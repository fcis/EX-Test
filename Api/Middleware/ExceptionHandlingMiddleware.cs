using System.Net;
using System.Text.Json;
using Application.Exceptions;
using Core.Common;

namespace Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var response = new ApiResponse<string>();

            switch (exception)
            {
                case ValidationException validationEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response = ApiResponse<string>.ErrorResponse(validationEx.Errors, "Validation error occurred");
                    break;

                case NotFoundException notFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    response = ApiResponse<string>.ErrorResponse(notFoundEx.Message);
                    break;

                case BadRequestException badRequestEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response = ApiResponse<string>.ErrorResponse(badRequestEx.Message);
                    break;

                case UnauthorizedException unauthorizedEx:
                    statusCode = HttpStatusCode.Unauthorized;
                    response = ApiResponse<string>.ErrorResponse(unauthorizedEx.Message);
                    break;

                case ForbiddenException forbiddenEx:
                    statusCode = HttpStatusCode.Forbidden;
                    response = ApiResponse<string>.ErrorResponse(forbiddenEx.Message);
                    break;

                case AlreadyExistsException alreadyExistsEx:
                    statusCode = HttpStatusCode.Conflict;
                    response = ApiResponse<string>.ErrorResponse(alreadyExistsEx.Message);
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    response = ApiResponse<string>.ErrorResponse(
                        _environment.IsDevelopment()
                            ? exception.Message
                            : "An error occurred while processing your request.");
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }

    // Extension method to add the middleware to the HTTP pipeline
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}