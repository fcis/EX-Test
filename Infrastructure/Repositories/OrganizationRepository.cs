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
    public class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Organization?> GetOrganizationWithDepartmentsAsync(long organizationId)
        {
            return await _dbContext.Organizations
                .Include(o => o.Departments.Where(d => !d.Deleted))
                .SingleOrDefaultAsync(o => o.Id == organizationId && o.Status != Core.Enums.OrganizationStatus.DELETED);
        }

        public async Task<Organization?> GetOrganizationWithMembershipsAsync(long organizationId)
        {
            return await _dbContext.Organizations
                .Include(o => o.Memberships.Where(m => m.Status != Core.Enums.OrganizationMembershipStatus.DELETED))
                    .ThenInclude(m => m.Framework)
                .Include(o => o.Memberships.Where(m => m.Status != Core.Enums.OrganizationMembershipStatus.DELETED))
                    .ThenInclude(m => m.FrameworkVersion)
                .SingleOrDefaultAsync(o => o.Id == organizationId && o.Status != Core.Enums.OrganizationStatus.DELETED);
        }

        public async Task<Organization?> GetOrganizationWithUsersAsync(long organizationId)
        {
            return await _dbContext.Organizations
                .Include(o => o.Users)
                    .ThenInclude(u => u.Role)
                .SingleOrDefaultAsync(o => o.Id == organizationId && o.Status != Core.Enums.OrganizationStatus.DELETED);
        }

        public async Task<PagedList<Organization>> GetOrganizationsWithFilterAsync(PagingParameters pagingParameters, string? searchTerm = null)
        {
            var query = _dbContext.Organizations
                .Include(o => o.Departments.Where(d => !d.Deleted))
                .Include(o => o.Users)
                .Include(o => o.Memberships.Where(m => m.Status != Core.Enums.OrganizationMembershipStatus.DELETED))
                .Where(o => o.Status != Core.Enums.OrganizationStatus.DELETED);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(searchTerm) ||
                                    (o.Email != null && o.Email.ToLower().Contains(searchTerm)) ||
                                    (o.Industry != null && o.Industry.ToLower().Contains(searchTerm)));
            }

            // Apply search from PagingParameters
            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(search) ||
                                    (o.Email != null && o.Email.ToLower().Contains(search)) ||
                                    (o.Industry != null && o.Industry.ToLower().Contains(search)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "name":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(o => o.Name) : query.OrderBy(o => o.Name);
                        break;
                    case "email":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(o => o.Email) : query.OrderBy(o => o.Email);
                        break;
                    case "industry":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(o => o.Industry) : query.OrderBy(o => o.Industry);
                        break;
                    case "creationdate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(o => o.CreationDate) : query.OrderBy(o => o.CreationDate);
                        break;
                    default:
                        query = query.OrderBy(o => o.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(o => o.Name);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<Organization>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}