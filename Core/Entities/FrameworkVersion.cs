using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("framework_version")]
    public class FrameworkVersion : BaseEntity
    {
        public long FrameworkId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FrameworkVersionStatus Status { get; set; }
        public DateTime VersionDate { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual Framework Framework { get; set; } = null!;
        public virtual ICollection<FrameworkCategories> Categories { get; set; } = new List<FrameworkCategories>();
    }
}
