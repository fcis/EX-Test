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
    public class OrganizationDepartmentsRepository : GenericRepository<OrganizationDepartments>, IOrganizationDepartmentsRepository
    {
        public OrganizationDepartmentsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<OrganizationDepartments>> GetDepartmentsByOrganizationAsync(long organizationId)
        {
            return await _dbContext.OrganizationDepartments
                .Where(d => d.OrganizationId == organizationId && !d.Deleted)
                .ToListAsync();
        }

        public async Task<PagedList<OrganizationDepartments>> GetPagedDepartmentsAsync(long organizationId, PagingParameters pagingParameters)
        {
            var query = _dbContext.OrganizationDepartments
                .Include(d => d.Users)
                .Where(d => d.OrganizationId == organizationId && !d.Deleted);

            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var search = pagingParameters.SearchTerm.ToLower();
                query = query.Where(d => d.Name.ToLower().Contains(search));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "name":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name);
                        break;
                    case "creationdate":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(d => d.CreationDate) : query.OrderBy(d => d.CreationDate);
                        break;
                    default:
                        query = query.OrderBy(d => d.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(d => d.Name);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<OrganizationDepartments>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}