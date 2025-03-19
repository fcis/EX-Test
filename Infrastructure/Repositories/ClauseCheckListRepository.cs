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
    public class ClauseCheckListRepository : GenericRepository<ClauseCheckList>, IClauseCheckListRepository
    {
        public ClauseCheckListRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<ClauseCheckList>> GetCheckListsByClauseAsync(long clauseId)
        {
            return await _dbContext.ClauseCheckLists
                .Where(c => c.ClauseId == clauseId && !c.Deleted)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ClauseCheckList>> GetActiveCheckListsByClauseAsync(long clauseId)
        {
            return await _dbContext.ClauseCheckLists
                .Where(c => c.ClauseId == clauseId && !c.Deleted)
                .ToListAsync();
        }

        public async Task<PagedList<ClauseCheckList>> GetPagedCheckListsAsync(long clauseId, PagingParameters pagingParameters)
        {
            var query = _dbContext.ClauseCheckLists
                .Where(c => c.ClauseId == clauseId && !c.Deleted);

            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(search));
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

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<ClauseCheckList>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}