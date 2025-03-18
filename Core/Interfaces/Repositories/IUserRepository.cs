using Core.Common;
using Core.Entities.Identitiy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IReadOnlyList<User>> GetUsersByOrganizationAsync(long organizationId, PagingParameters pagingParameters);
        Task<bool> CheckPasswordAsync(string username, string password);
        Task<(string Token, DateTime Expiry)> GenerateJwtTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync(User user);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
        Task<PagedList<User>> GetPagedUsersAsync(PagingParameters pagingParameters);
    }
}
