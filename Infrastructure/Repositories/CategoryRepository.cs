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
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Category>> GetCategoriesByFrameworkVersionAsync(long frameworkVersionId)
        {
            return await _dbContext.FrameworkCategories
                .Where(fc => fc.FrameworkVersionId == frameworkVersionId && !fc.Deleted)
                .Select(fc => fc.Category)
                .Where(c => !c.Deleted)
                .ToListAsync();
        }

        public async Task<PagedList<Category>> GetPagedCategoriesAsync(PagingParameters pagingParameters)
        {
            // Start with base query without includes
            var query = _dbContext.Categories
                .Where(c => !c.Deleted);

            // Apply search if provided
            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var searchTerm = $"%{pagingParameters.SearchTerm}%";
                query = query.Where(c =>
                    EF.Functions.Like(c.Name, searchTerm) ||
                    (c.Description != null && EF.Functions.Like(c.Description, searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "name":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name);
                        break;
                    case "creationdate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(c => c.CreationDate) : query.OrderBy(c => c.CreationDate);
                        break;
                    default:
                        query = query.OrderBy(c => c.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(c => c.Name);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply includes after filtering
            var queryWithIncludes = query.Include(c => c.Frameworks.Where(f => !f.Deleted));

            // Materialize query
            var items = await queryWithIncludes
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<Category>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}