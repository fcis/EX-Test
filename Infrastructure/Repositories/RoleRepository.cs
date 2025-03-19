using Core.Common;
using Core.Entities.Identitiy;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Role?> GetRoleWithPermissionsAsync(long roleId)
        {
            return await _dbContext.Roles
                .Include(r => r.Permissions)
                    .ThenInclude(p => p.Permission)
                .SingleOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task<IReadOnlyList<Permission>> GetRolePermissionsAsync(long roleId)
        {
            var role = await GetRoleWithPermissionsAsync(roleId);
            if (role == null)
            {
                return new List<Permission>();
            }

            return role.Permissions.Select(p => p.Permission).ToList();
        }

        public async Task<bool> AddPermissionToRoleAsync(long roleId, long permissionId)
        {
            var rolePermission = await _dbContext.RolePermissions
                .SingleOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission != null)
            {
                // Permission already exists
                return false;
            }

            rolePermission = new RolePermissions
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            _dbContext.RolePermissions.Add(rolePermission);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(long roleId, long permissionId)
        {
            var rolePermission = await _dbContext.RolePermissions
                .SingleOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission == null)
            {
                // Permission doesn't exist
                return false;
            }

            _dbContext.RolePermissions.Remove(rolePermission);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<PagedList<Role>> GetPagedRolesAsync(PagingParameters pagingParameters)
        {
            // Start with base query without includes
            var query = _dbContext.Roles.AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var searchTerm = $"%{pagingParameters.SearchTerm}%";
                query = query.Where(r => EF.Functions.Like(r.Name, searchTerm));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "name":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name);
                        break;
                    case "portal":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(r => r.Portal) : query.OrderBy(r => r.Portal);
                        break;
                    default:
                        query = query.OrderBy(r => r.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(r => r.Name);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply includes after filtering
            var queryWithIncludes = query
                .Include(r => r.Permissions)
                    .ThenInclude(p => p.Permission);

            // Materialize query
            var items = await queryWithIncludes
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<Role>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}