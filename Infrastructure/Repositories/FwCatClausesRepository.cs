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
    public class FwCatClausesRepository : GenericRepository<FwCatClauses>, IFwCatClausesRepository
    {
        public FwCatClausesRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<FwCatClauses>> GetByFrameworkCategoryAsync(long frameworkCategoryId)
        {
            return await _dbContext.FwCatClauses
                .Include(fc => fc.Clause)
                .Where(fc => fc.FrameworkCategoryId == frameworkCategoryId && !fc.Deleted)
                .ToListAsync();
        }

        public async Task<PagedList<FwCatClauses>> GetPagedClausesAsync(long frameworkCategoryId, PagingParameters pagingParameters)
        {
            var query = _dbContext.FwCatClauses
                .Include(fc => fc.Clause)
                .Where(fc => fc.FrameworkCategoryId == frameworkCategoryId && !fc.Deleted);

            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(fc => fc.Clause.Name.ToLower().Contains(search) ||
                                    (fc.ClauseNumber != null && fc.ClauseNumber.ToLower().Contains(search)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "clausenumber":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(fc => fc.ClauseNumber) : query.OrderBy(fc => fc.ClauseNumber);
                        break;
                    case "clausename":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(fc => fc.Clause.Name) : query.OrderBy(fc => fc.Clause.Name);
                        break;
                    default:
                        query = query.OrderBy(fc => fc.ClauseNumber).ThenBy(fc => fc.Clause.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(fc => fc.ClauseNumber).ThenBy(fc => fc.Clause.Name);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<FwCatClauses>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}