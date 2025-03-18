using Core.Entities.Identitiy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("organization_departments")]
    public class OrganizationDepartments : BaseEntity
    {
        public long OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
