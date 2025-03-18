using Core.Entities.Identitiy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Authentication
{
    public interface IJwtService
    {
        string GenerateToken(User user, List<string> roles, List<string> permissions);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        long? GetUserIdFromToken(string token);
    }
}
