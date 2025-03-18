using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Identitiy
{
    [Table("user")]
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long RoleId { get; set; }
        public long? OrganizationId { get; set; }
        public long? OrganizationDepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenValidity { get; set; }
        public UserStatus Status { get; set; }
        public int WrongPasswordCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long? LastModificationUser { get; set; }

        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual Organization? Organization { get; set; }
        public virtual OrganizationDepartments? OrganizationDepartment { get; set; }
    }
}
