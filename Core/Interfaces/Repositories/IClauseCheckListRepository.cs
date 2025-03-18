using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IClauseCheckListRepository : IGenericRepository<ClauseCheckList>
    {
        Task<IReadOnlyList<ClauseCheckList>> GetCheckListsByClauseAsync(long clauseId);
        Task<IReadOnlyList<ClauseCheckList>> GetActiveCheckListsByClauseAsync(long clauseId);
        Task<PagedList<ClauseCheckList>> GetPagedCheckListsAsync(long clauseId, PagingParameters pagingParameters);
    }
}
