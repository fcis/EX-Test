using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("framework_categories")]
    public class FrameworkCategories : BaseEntity
    {
        public long FrameworkVersionId { get; set; }
        public long CategoryId { get; set; }
        public string? CategoryNumber { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual FrameworkVersion FrameworkVersion { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<FwCatClauses> Clauses { get; set; } = new List<FwCatClauses>();
    }
}

