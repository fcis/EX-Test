using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public long? OrganizationId { get; set; }
        public long? OrganizationDepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public UserStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedAt { get; set; }
        public long? CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long? LastModificationUser { get; set; }
    }

    public class UserListDto
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? OrganizationName { get; set; }
        public UserStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RoleDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Portal Portal { get; set; }
        public string PortalName => Portal.ToString();
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }

    public class PermissionDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
