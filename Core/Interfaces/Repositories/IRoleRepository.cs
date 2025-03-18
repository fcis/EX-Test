using Core.Common;
using Core.Entities.Identitiy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetRoleWithPermissionsAsync(long roleId);
        Task<IReadOnlyList<Permission>> GetRolePermissionsAsync(long roleId);
        Task<bool> AddPermissionToRoleAsync(long roleId, long permissionId);
        Task<bool> RemovePermissionFromRoleAsync(long roleId, long permissionId);
        Task<PagedList<Role>> GetPagedRolesAsync(PagingParameters pagingParameters);
    }
}
