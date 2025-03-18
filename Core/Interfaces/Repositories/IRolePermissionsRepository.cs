using Core.Common;
using Core.Entities.Identitiy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IRolePermissionsRepository : IGenericRepository<RolePermissions>
    {
        Task<IReadOnlyList<RolePermissions>> GetByRoleAsync(long roleId);
        Task<PagedList<RolePermissions>> GetPagedRolePermissionsAsync(long roleId, PagingParameters pagingParameters);
    }
}
