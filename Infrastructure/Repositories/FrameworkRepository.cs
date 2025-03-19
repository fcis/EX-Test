// Repositories/FrameworkRepository.cs
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
    public class FrameworkRepository : GenericRepository<Framework>, IFrameworkRepository
    {
        public FrameworkRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Framework?> GetFrameworkWithVersionsAsync(long frameworkId)
        {
            return await _dbContext.Frameworks
                .Include(f => f.Versions)
                .SingleOrDefaultAsync(f => f.Id == frameworkId);
        }

        public async Task<Framework?> GetFrameworkWithFullDetailAsync(long frameworkId)
        {
            return await _dbContext.Frameworks
                .Include(f => f.Versions)
                    .ThenInclude(v => v.Categories)
                        .ThenInclude(c => c.Category)
                .Include(f => f.Versions)
                    .ThenInclude(v => v.Categories)
                        .ThenInclude(c => c.Clauses)
                            .ThenInclude(cl => cl.Clause)
                                .ThenInclude(clause => clause.CheckLists)
                .SingleOrDefaultAsync(f => f.Id == frameworkId);
        }

        public async Task<PagedList<Framework>> GetActiveFrameworksAsync(PagingParameters pagingParameters)
        {
            var query = _dbContext.Frameworks
                .Include(f => f.Versions)
                .Where(f => f.Status == Core.Enums.FrameworkStatus.ACTIVE);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<Framework>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}