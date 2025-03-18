using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IClauseRepository : IGenericRepository<Clause>
    {
        Task<IReadOnlyList<Clause>> GetClausesByCategoryAsync(long categoryId);
        Task<PagedList<Clause>> GetPagedClausesAsync(PagingParameters pagingParameters);
    }
}
