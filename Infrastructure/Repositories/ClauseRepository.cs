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
    public class ClauseRepository : GenericRepository<Clause>, IClauseRepository
    {
        public ClauseRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Clause>> GetClausesByCategoryAsync(long categoryId)
        {
            return await _dbContext.FwCatClauses
                .Where(fc => fc.FrameworkCategory.CategoryId == categoryId && !fc.Deleted)
                .Select(fc => fc.Clause)
                .Where(c => !c.Deleted)
                .Distinct()
                .ToListAsync();
        }

        public async Task<PagedList<Clause>> GetPagedClausesAsync(PagingParameters pagingParameters)
        {
            // Start with a base query without includes
            var query = _dbContext.Clauses
                .Where(c => !c.Deleted);

            // Apply search if provided
            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var searchTerm = $"%{pagingParameters.SearchTerm}%";
                query = query.Where(c => EF.Functions.Like(c.Name, searchTerm) ||
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

            // Get total count before includes
            var totalCount = await query.CountAsync();

            // Apply includes AFTER all filtering
            var queryWithIncludes = query.Include(c => c.CheckLists.Where(cl => !cl.Deleted));

            // Materialize the query
            var items = await queryWithIncludes
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<Clause>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}