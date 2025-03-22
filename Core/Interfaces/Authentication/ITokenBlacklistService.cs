using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Authentication
{
    public interface ITokenBlacklistService
    {
        Task AddToBlacklistAsync(string token, DateTime expiry);
        Task<bool> IsBlacklistedAsync(string token);
        Task RemoveExpiredTokensAsync();
    }
}
