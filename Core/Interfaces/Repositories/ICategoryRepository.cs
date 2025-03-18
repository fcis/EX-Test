using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IReadOnlyList<Category>> GetCategoriesByFrameworkVersionAsync(long frameworkVersionId);
        Task<PagedList<Category>> GetPagedCategoriesAsync(PagingParameters pagingParameters);
    }
}
