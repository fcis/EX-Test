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
    public class OrganizationClauseAnswersRepository : GenericRepository<OrganizationClauseAnswers>, IOrganizationClauseAnswersRepository
    {
        public OrganizationClauseAnswersRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<OrganizationClauseAnswers>> GetByOrganizationAsync(long organizationId)
        {
            return await _dbContext.OrganizationClauseAnswers
                .Include(a => a.Clause)
                .Where(a => a.OrganizationId == organizationId && !a.Deleted)
                .ToListAsync();
        }

        public async Task<OrganizationClauseAnswers?> GetByOrganizationAndClauseAsync(long organizationId, long clauseId)
        {
            return await _dbContext.OrganizationClauseAnswers
                .Include(a => a.Clause)
                .SingleOrDefaultAsync(a => a.OrganizationId == organizationId &&
                                     a.ClauseId == clauseId &&
                                     !a.Deleted);
        }

        public async Task<PagedList<OrganizationClauseAnswers>> GetPagedAnswersAsync(long organizationId, PagingParameters pagingParameters)
        {
            var query = _dbContext.OrganizationClauseAnswers
                .Include(a => a.Clause)
                .Where(a => a.OrganizationId == organizationId && !a.Deleted);

            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(a => a.Clause.Name.ToLower().Contains(search));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "clausename":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.Clause.Name) : query.OrderBy(a => a.Clause.Name);
                        break;
                    case "status":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status);
                        break;
                    case "creationdate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.CreationDate) : query.OrderBy(a => a.CreationDate);
                        break;
                    default:
                        query = query.OrderBy(a => a.Clause.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(a => a.Clause.Name);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<OrganizationClauseAnswers>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}