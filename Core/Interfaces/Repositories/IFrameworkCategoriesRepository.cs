using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IFrameworkCategoriesRepository : IGenericRepository<FrameworkCategories>
    {
        Task<IReadOnlyList<FrameworkCategories>> GetByFrameworkVersionAsync(long frameworkVersionId);
        Task<PagedList<FrameworkCategories>> GetPagedFrameworkCategoriesAsync(long frameworkVersionId, PagingParameters pagingParameters);
    }
}
