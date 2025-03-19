using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class OrganizationMembershipRepository : GenericRepository<OrganizationMembership>, IOrganizationMembershipRepository
    {
        public OrganizationMembershipRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<OrganizationMembership>> GetByOrganizationAsync(long organizationId)
        {
            return await _dbContext.OrganizationMemberships
                .Include(m => m.Framework)
                .Include(m => m.FrameworkVersion)
                .Where(m => m.OrganizationId == organizationId && m.Status != OrganizationMembershipStatus.DELETED)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<OrganizationMembership>> GetByFrameworkAsync(long frameworkId)
        {
            return await _dbContext.OrganizationMemberships
                .Include(m => m.Organization)
                .Include(m => m.FrameworkVersion)
                .Where(m => m.FrameworkId == frameworkId && m.Status != OrganizationMembershipStatus.DELETED)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<OrganizationMembership>> GetByFrameworkVersionAsync(long frameworkVersionId)
        {
            return await _dbContext.OrganizationMemberships
                .Include(m => m.Organization)
                .Include(m => m.Framework)
                .Where(m => m.FrameworkVersionId == frameworkVersionId && m.Status != OrganizationMembershipStatus.DELETED)
                .ToListAsync();
        }

        public async Task<OrganizationMembership?> GetByOrganizationAndFrameworkAsync(long organizationId, long frameworkId)
        {
            return await _dbContext.OrganizationMemberships
                .Include(m => m.Framework)
                .Include(m => m.FrameworkVersion)
                .Where(m => m.OrganizationId == organizationId &&
                       m.FrameworkId == frameworkId &&
                       m.Status != OrganizationMembershipStatus.DELETED)
                .OrderByDescending(m => m.CreationDate)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedList<OrganizationMembership>> GetPagedMembershipsAsync(
            long organizationId,
            PagingParameters pagingParameters,
            OrganizationMembershipStatus? status = null)
        {
            var query = _dbContext.OrganizationMemberships
                .Include(m => m.Framework)
                .Include(m => m.FrameworkVersion)
                .Where(m => m.OrganizationId == organizationId && m.Status != OrganizationMembershipStatus.DELETED);

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(m => m.Framework.Name.ToLower().Contains(search) ||
                                    m.FrameworkVersion.Name.ToLower().Contains(search));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "frameworkname":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(m => m.Framework.Name) : query.OrderBy(m => m.Framework.Name);
                        break;
                    case "versionname":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(m => m.FrameworkVersion.Name) : query.OrderBy(m => m.FrameworkVersion.Name);
                        break;
                    case "status":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(m => m.Status) : query.OrderBy(m => m.Status);
                        break;
                    case "creationdate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(m => m.CreationDate) : query.OrderBy(m => m.CreationDate);
                        break;
                    default:
                        query = query.OrderByDescending(m => m.CreationDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(m => m.CreationDate);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<OrganizationMembership>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<bool> HasActiveFrameworkMembershipAsync(long organizationId, long frameworkId)
        {
            return await _dbContext.OrganizationMemberships
                .AnyAsync(m => m.OrganizationId == organizationId &&
                         m.FrameworkId == frameworkId &&
                         m.Status == OrganizationMembershipStatus.ACTIVE);
        }
    }
}