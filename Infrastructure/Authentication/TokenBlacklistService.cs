using Core.Interfaces.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Authentication
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<TokenBlacklistService> _logger;

        public TokenBlacklistService(IDistributedCache cache, ILogger<TokenBlacklistService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task AddToBlacklistAsync(string token, DateTime expiry)
        {
            try
            {
                var timeSpan = expiry - DateTime.UtcNow;
                if (timeSpan.TotalSeconds <= 0)
                    return; // Token already expired, no need to blacklist

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = timeSpan
                };

                await _cache.SetStringAsync($"blacklisted_token_{token}", "1", options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding token to blacklist");
            }
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            try
            {
                var value = await _cache.GetStringAsync($"blacklisted_token_{token}");
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if token is blacklisted");
                return false;
            }
        }

        public async Task RemoveExpiredTokensAsync()
        {
            // No need for implementation as IDistributedCache automatically removes expired entries
            await Task.CompletedTask;
        }
    }
}