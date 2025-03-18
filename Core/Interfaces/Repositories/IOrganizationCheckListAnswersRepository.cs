using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IOrganizationCheckListAnswersRepository : IGenericRepository<OrganizationCheckListAnswers>
    {
        Task<IReadOnlyList<OrganizationCheckListAnswers>> GetByOrganizationAsync(long organizationId);
        Task<OrganizationCheckListAnswers?> GetByOrganizationAndCheckListAsync(long organizationId, long checkListId);
        Task<PagedList<OrganizationCheckListAnswers>> GetPagedAnswersAsync(long organizationId, PagingParameters pagingParameters);
    }
}
