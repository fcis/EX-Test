using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("audit")]
    public class Audit : BaseEntity
    {
        public long? User { get; set; }
        public AuditAction Action { get; set; }
        public DateTime When { get; set; }
        public string Ip { get; set; } = string.Empty;
        public string? Entity { get; set; }
        public long? EntityId { get; set; }
        public string? Details { get; set; }
    }
}
