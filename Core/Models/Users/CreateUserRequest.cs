using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Users
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Add status field
        public UserStatus Status { get; set; } = UserStatus.NEW; // Default to NEW if not specified

        [Required]
        public long RoleId { get; set; }

        public long? OrganizationId { get; set; }

        public long? OrganizationDepartmentId { get; set; }
    }
}
