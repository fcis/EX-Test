using Core.Common;
using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class FrameworkCategoriesRepository : GenericRepository<FrameworkCategories>, IFrameworkCategoriesRepository
    {
        public FrameworkCategoriesRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<FrameworkCategories>> GetByFrameworkVersionAsync(long frameworkVersionId)
        {
            return await _dbContext.FrameworkCategories
                .Include(fc => fc.Category)
                .Where(fc => fc.FrameworkVersionId == frameworkVersionId && !fc.Deleted)
                .ToListAsync();
        }

        public async Task<PagedList<FrameworkCategories>> GetPagedFrameworkCategoriesAsync(long frameworkVersionId, PagingParameters pagingParameters)
        {
            var query = _dbContext.FrameworkCategories
                .Include(fc => fc.Category)
                .Include(fc => fc.Clauses)
                .Where(fc => fc.FrameworkVersionId == frameworkVersionId && !fc.Deleted);

            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(fc => fc.Category.Name.ToLower().Contains(search) ||
                                     (fc.CategoryNumber != null && fc.CategoryNumber.ToLower().Contains(search)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "categorynumber":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(fc => fc.CategoryNumber) : query.OrderBy(fc => fc.CategoryNumber);
                        break;
                    case "categoryname":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(fc => fc.Category.Name) : query.OrderBy(fc => fc.Category.Name);
                        break;
                    default:
                        query = query.OrderBy(fc => fc.CategoryNumber).ThenBy(fc => fc.Category.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(fc => fc.CategoryNumber).ThenBy(fc => fc.Category.Name);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<FrameworkCategories>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}