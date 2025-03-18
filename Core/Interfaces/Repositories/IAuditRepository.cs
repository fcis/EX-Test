using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IAuditRepository : IGenericRepository<Audit>
    {
        Task<IReadOnlyList<Audit>> GetAuditsByUserAsync(long userId);
        Task<IReadOnlyList<Audit>> GetAuditsByEntityAsync(string entityName, long entityId);
        Task<PagedList<Audit>> GetPagedAuditsAsync(PagingParameters pagingParameters);
    }
}
