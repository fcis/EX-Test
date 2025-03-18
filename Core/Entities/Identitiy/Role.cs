using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Identitiy
{
    [Table("role")]
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public Portal Portal { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<RolePermissions> Permissions { get; set; } = new List<RolePermissions>();
    }
}
