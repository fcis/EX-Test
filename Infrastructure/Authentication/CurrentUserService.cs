// Authentication/CurrentUserService.cs
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier) ??
                                  _httpContextAccessor.HttpContext?.User.FindFirst("sub");

                if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
                {
                    return userId;
                }

                return null;
            }
        }

        public string? UserName
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User.FindFirst("username")?.Value;
            }
        }

        public string? IpAddress
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

                // Check for forwarded IP (if behind a load balancer or proxy)
                var forwardedFor = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // The X-Forwarded-For header can contain multiple IP addresses in a comma-separated list
                    // The leftmost IP address is the original client IP
                    ipAddress = forwardedFor.Split(',')[0].Trim();
                }

                return ipAddress ?? "Unknown";
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
        }
    }
}