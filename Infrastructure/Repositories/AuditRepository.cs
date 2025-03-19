using Core.Common;
using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AuditRepository : GenericRepository<Audit>, IAuditRepository
    {
        public AuditRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Audit>> GetAuditsByUserAsync(long userId)
        {
            return await _dbContext.Audits
                .Where(a => a.User == userId)
                .OrderByDescending(a => a.When)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Audit>> GetAuditsByEntityAsync(string entityName, long entityId)
        {
            return await _dbContext.Audits
                .Where(a => a.Entity == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.When)
                .ToListAsync();
        }

        public async Task<PagedList<Audit>> GetPagedAuditsAsync(PagingParameters pagingParameters)
        {
            // Start with base query without includes
            var query = _dbContext.Audits.AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                var searchTerm = $"%{pagingParameters.SearchTerm}%";
                query = query.Where(a =>
                    (a.Entity != null && EF.Functions.Like(a.Entity, searchTerm)) ||
                    (a.Details != null && EF.Functions.Like(a.Details, searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                switch (pagingParameters.SortColumn.ToLower())
                {
                    case "entity":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.Entity) : query.OrderBy(a => a.Entity);
                        break;
                    case "action":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.Action) : query.OrderBy(a => a.Action);
                        break;
                    case "when":
                        query = pagingParameters.SortOrder?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.When) : query.OrderBy(a => a.When);
                        break;
                    default:
                        query = query.OrderByDescending(a => a.When);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(a => a.When);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Materialize query
            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<Audit>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}