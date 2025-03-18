using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Audit
{
    public class AuditDto
    {
        public long Id { get; set; }
        public long? User { get; set; }
        public string? UserName { get; set; }
        public AuditAction Action { get; set; }
        public string ActionName => Action.ToString();
        public DateTime When { get; set; }
        public string Ip { get; set; } = string.Empty;
        public string? Entity { get; set; }
        public long? EntityId { get; set; }
        public string? Details { get; set; }
    }
}
