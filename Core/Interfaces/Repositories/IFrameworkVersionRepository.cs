using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IFrameworkVersionRepository : IGenericRepository<FrameworkVersion>
    {
        Task<FrameworkVersion?> GetVersionWithCategoriesAsync(long versionId);
        Task<IReadOnlyList<FrameworkVersion>> GetVersionsByFrameworkAsync(long frameworkId);
        Task<PagedList<FrameworkVersion>> GetPagedVersionsAsync(long frameworkId, PagingParameters pagingParameters);
    }
}
