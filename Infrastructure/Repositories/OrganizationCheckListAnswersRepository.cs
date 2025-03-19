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
    public class OrganizationCheckListAnswersRepository : GenericRepository<OrganizationCheckListAnswers>, IOrganizationCheckListAnswersRepository
    {
        public OrganizationCheckListAnswersRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<OrganizationCheckListAnswers>> GetByOrganizationAsync(long organizationId)
        {
            return await _dbContext.OrganizationCheckListAnswers
                .Where(a => a.OrganizationId == organizationId && !a.Deleted)
                .ToListAsync();
        }

        public async Task<OrganizationCheckListAnswers?> GetByOrganizationAndCheckListAsync(long organizationId, long checkListId)
        {
            return await _dbContext.OrganizationCheckListAnswers
                .SingleOrDefaultAsync(a => a.OrganizationId == organizationId &&
                                     a.CheckId == checkListId &&
                                     !a.Deleted);
        }

        public async Task<PagedList<OrganizationCheckListAnswers>> GetPagedAnswersAsync(long organizationId, PagingParameters pagingParameters)
        {
            var query = _dbContext.OrganizationCheckListAnswers
                .Where(a => a.OrganizationId == organizationId && !a.Deleted);

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "done":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.Done) : query.OrderBy(a => a.Done);
                        break;
                    case "creationdate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.CreationDate) : query.OrderBy(a => a.CreationDate);
                        break;
                    default:
                        query = query.OrderBy(a => a.CheckId);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(a => a.CheckId);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<OrganizationCheckListAnswers>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}