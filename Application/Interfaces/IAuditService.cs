using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface for the audit service
    /// </summary>
    public interface IAuditService
    {
        Task LogActionAsync(string entity, long? entityId, string action, string? details = null);
    }
}
