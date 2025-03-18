using Core.Common;
using Core.Entities.Identitiy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IPermissionRepository : IGenericRepository<Permission>
    {
        Task<PagedList<Permission>> GetPagedPermissionsAsync(PagingParameters pagingParameters);
    }
}
