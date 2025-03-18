using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IFrameworkRepository : IGenericRepository<Framework>
    {
        Task<Framework?> GetFrameworkWithVersionsAsync(long frameworkId);
        Task<Framework?> GetFrameworkWithFullDetailAsync(long frameworkId);
        Task<PagedList<Framework>> GetActiveFrameworksAsync(PagingParameters pagingParameters);
    }
}
