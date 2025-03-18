using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("framework")]
    public class Framework : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FrameworkStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual ICollection<FrameworkVersion> Versions { get; set; } = new List<FrameworkVersion>();
    }
}
