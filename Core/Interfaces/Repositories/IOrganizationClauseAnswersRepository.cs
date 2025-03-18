using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IOrganizationClauseAnswersRepository : IGenericRepository<OrganizationClauseAnswers>
    {
        Task<IReadOnlyList<OrganizationClauseAnswers>> GetByOrganizationAsync(long organizationId);
        Task<OrganizationClauseAnswers?> GetByOrganizationAndClauseAsync(long organizationId, long clauseId);
        Task<PagedList<OrganizationClauseAnswers>> GetPagedAnswersAsync(long organizationId, PagingParameters pagingParameters);
    }
}
