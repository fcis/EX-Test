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
    public class FrameworkVersionRepository : GenericRepository<FrameworkVersion>, IFrameworkVersionRepository
    {
        public FrameworkVersionRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<FrameworkVersion?> GetVersionWithCategoriesAsync(long versionId)
        {
            return await _dbContext.FrameworkVersions
                .Include(v => v.Categories)
                    .ThenInclude(c => c.Category)
                .SingleOrDefaultAsync(v => v.Id == versionId);
        }

        public async Task<IReadOnlyList<FrameworkVersion>> GetVersionsByFrameworkAsync(long frameworkId)
        {
            return await _dbContext.FrameworkVersions
                .Where(v => v.FrameworkId == frameworkId && v.Status != Core.Enums.FrameworkVersionStatus.DELETED)
                .OrderByDescending(v => v.VersionDate)
                .ToListAsync();
        }

        public async Task<PagedList<FrameworkVersion>> GetPagedVersionsAsync(long frameworkId, PagingParameters pagingParameters)
        {
            var query = _dbContext.FrameworkVersions
                .Where(v => v.FrameworkId == frameworkId && v.Status != Core.Enums.FrameworkVersionStatus.DELETED);

            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(v => v.Name.ToLower().Contains(search) ||
                                    (v.Description != null && v.Description.ToLower().Contains(search)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "name":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(v => v.Name) : query.OrderBy(v => v.Name);
                        break;
                    case "versiondate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(v => v.VersionDate) : query.OrderBy(v => v.VersionDate);
                        break;
                    case "creationdate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(v => v.CreationDate) : query.OrderBy(v => v.CreationDate);
                        break;
                    default:
                        query = query.OrderByDescending(v => v.VersionDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(v => v.VersionDate);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<FrameworkVersion>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}