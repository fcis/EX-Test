using Core.Common;
using Core.Entities;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IOrganizationMembershipRepository : IGenericRepository<OrganizationMembership>
    {
        Task<IReadOnlyList<OrganizationMembership>> GetByOrganizationAsync(long organizationId);
        Task<IReadOnlyList<OrganizationMembership>> GetByFrameworkAsync(long frameworkId);
        Task<IReadOnlyList<OrganizationMembership>> GetByFrameworkVersionAsync(long frameworkVersionId);
        Task<OrganizationMembership?> GetByOrganizationAndFrameworkAsync(long organizationId, long frameworkId);
        Task<PagedList<OrganizationMembership>> GetPagedMembershipsAsync(
            long organizationId,
            PagingParameters pagingParameters,
            OrganizationMembershipStatus? status = null);
        Task<bool> HasActiveFrameworkMembershipAsync(long organizationId, long frameworkId);
    }
}
