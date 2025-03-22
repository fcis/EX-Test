// Infrastructure/Authentication/CurrentTokenProvider.cs
using Core.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Infrastructure.Authentication
{
    public class CurrentTokenProvider : ICurrentTokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentTokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentToken()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                .FirstOrDefault()?.Replace("Bearer ", "");
        }
    }
}