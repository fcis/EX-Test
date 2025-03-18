using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IOrganizationDepartmentsRepository : IGenericRepository<OrganizationDepartments>
    {
        Task<IReadOnlyList<OrganizationDepartments>> GetDepartmentsByOrganizationAsync(long organizationId);
        Task<PagedList<OrganizationDepartments>> GetPagedDepartmentsAsync(long organizationId, PagingParameters pagingParameters);
    }
}
