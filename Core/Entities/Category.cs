using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("category")]
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual ICollection<FrameworkCategories> Frameworks { get; set; } = new List<FrameworkCategories>();
    }
}
