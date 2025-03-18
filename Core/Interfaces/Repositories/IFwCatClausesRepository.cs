using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IFwCatClausesRepository : IGenericRepository<FwCatClauses>
    {
        Task<IReadOnlyList<FwCatClauses>> GetByFrameworkCategoryAsync(long frameworkCategoryId);
        Task<PagedList<FwCatClauses>> GetPagedClausesAsync(long frameworkCategoryId, PagingParameters pagingParameters);
    }
}
