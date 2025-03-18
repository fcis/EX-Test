using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Identitiy
{
    [Table("permission")]
    public class Permission : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<RolePermissions> Roles { get; set; } = new List<RolePermissions>();
    }
}
