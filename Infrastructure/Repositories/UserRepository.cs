using Core.Common;
using Core.Entities.Identitiy;
using Core.Interfaces;
using Core.Interfaces.Authentication;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IPasswordHasher _passwordHasher;

        public UserRepository(
            AppDbContext dbContext,
            IJwtTokenGenerator jwtTokenGenerator,
            IPasswordHasher passwordHasher) : base(dbContext)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .Include(u => u.OrganizationDepartment)
                .SingleOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .Include(u => u.OrganizationDepartment)
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IReadOnlyList<User>> GetUsersByOrganizationAsync(long organizationId, PagingParameters pagingParameters)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.OrganizationDepartment)
                .Where(u => u.OrganizationId == organizationId && u.Status != Core.Enums.UserStatus.DELETED)
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();
        }

        public async Task<bool> CheckPasswordAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
            {
                return false;
            }

            return _passwordHasher.VerifyPassword(user.Password, password);
        }

        public async Task<(string Token, DateTime Expiry)> GenerateJwtTokenAsync(User user)
        {
            // Load role and permissions
            var role = await _dbContext.Roles
                .Include(r => r.Permissions)
                    .ThenInclude(p => p.Permission)
                .SingleOrDefaultAsync(r => r.Id == user.RoleId);

            if (role == null)
            {
                throw new ApplicationException("User role not found");
            }

            var permissions = role.Permissions.Select(p => p.Permission.Name).ToList();

            var token = _jwtTokenGenerator.GenerateToken(user, new List<string> { role.Name }, permissions);
            var expiry = DateTime.UtcNow.AddHours(1); // Assuming 1 hour token expiry

            return (token, expiry);
        }

        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            user.Token = refreshToken;
            user.TokenValidity = DateTime.UtcNow.AddDays(7); // 7 days refresh token validity

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Token == refreshToken && u.TokenValidity > DateTime.UtcNow);

            return user != null;
        }

        public async Task<PagedList<User>> GetPagedUsersAsync(PagingParameters pagingParameters)
        {
            var query = _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .Where(u => u.Status != Core.Enums.UserStatus.DELETED);

            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(search) ||
                                    u.Email.ToLower().Contains(search) ||
                                    u.Name.ToLower().Contains(search));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "username":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username);
                        break;
                    case "email":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                        break;
                    case "name":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name);
                        break;
                    case "role":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(u => u.Role.Name) : query.OrderBy(u => u.Role.Name);
                        break;
                    case "status":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(u => u.Status) : query.OrderBy(u => u.Status);
                        break;
                    case "createdat":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt);
                        break;
                    default:
                        query = query.OrderBy(u => u.Username);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(u => u.Username);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<User>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}