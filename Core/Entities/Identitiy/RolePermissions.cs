using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Identitiy
{
    [Table("role_permissions")]
    public class RolePermissions : BaseEntity
    {
        public long RoleId { get; set; }
        public long PermissionId { get; set; }

        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}
