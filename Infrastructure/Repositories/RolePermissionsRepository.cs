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
    public class RolePermissionsRepository : GenericRepository<RolePermissions>, IRolePermissionsRepository
    {
        public RolePermissionsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<RolePermissions>> GetByRoleAsync(long roleId)
        {
            return await _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<PagedList<RolePermissions>> GetPagedRolePermissionsAsync(long roleId, PagingParameters pagingParameters)
        {
            var query = _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId);

            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(rp => rp.Permission.Name.ToLower().Contains(search));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "permissionname":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(rp => rp.Permission.Name) : query.OrderBy(rp => rp.Permission.Name);
                        break;
                    default:
                        query = query.OrderBy(rp => rp.Permission.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(rp => rp.Permission.Name);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<RolePermissions>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}