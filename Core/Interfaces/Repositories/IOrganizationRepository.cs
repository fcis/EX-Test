using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IOrganizationRepository : IGenericRepository<Organization>
    {
        Task<Organization?> GetOrganizationWithDepartmentsAsync(long organizationId);
        Task<Organization?> GetOrganizationWithMembershipsAsync(long organizationId);
        Task<Organization?> GetOrganizationWithUsersAsync(long organizationId);
        Task<PagedList<Organization>> GetOrganizationsWithFilterAsync(PagingParameters pagingParameters, string? searchTerm = null);
    }
}
