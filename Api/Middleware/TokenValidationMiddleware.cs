// Api/Middleware/TokenValidationMiddleware.cs
using Core.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;

namespace Api.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get the token from the request
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                // Resolve the service from the request scope
                var tokenBlacklistService = context.RequestServices.GetRequiredService<ITokenBlacklistService>();

                // Check if the token is blacklisted
                var isBlacklisted = await tokenBlacklistService.IsBlacklistedAsync(token);
                if (isBlacklisted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    // Create an ApiResponse object
                    var response = Core.Common.ApiResponse<string>.ErrorResponse(
                        "Token is invalid or has been revoked",
                        StatusCodes.Status401Unauthorized
                    );

                    // Serialize to JSON
                    var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    });

                    // Write the JSON response
                    await context.Response.WriteAsync(jsonResponse);
                    return;
                }
            }

            await _next(context);
        }
    }

    // Extension method for middleware registration
    public static class TokenValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenValidationMiddleware>();
        }
    }
}