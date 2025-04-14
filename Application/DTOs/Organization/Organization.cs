using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Organization
{
    public class OrganizationDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? EmployeesCount { get; set; }
        public string? Industry { get; set; }
        public string? Description { get; set; }
        public OrganizationStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }
        public List<OrganizationDepartmentDto> Departments { get; set; } = new List<OrganizationDepartmentDto>();
    }

    public class OrganizationListDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Industry { get; set; }
        public OrganizationStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreationDate { get; set; }
        public int DepartmentsCount { get; set; }
        public int UsersCount { get; set; }
        public int FrameworksCount { get; set; }
        public string logoUrl { get; set; } = String.Empty;
        public DateTime LastModificationDate { get; set; }
    }

    public class CreateOrganizationDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Url]
        public string? Website { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public int? EmployeesCount { get; set; }

        public string? Industry { get; set; }

        public string? Description { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one department is required")]
        public List<CreateOrganizationDepartmentDto> Departments { get; set; } = new List<CreateOrganizationDepartmentDto>();
    }

    public class UpdateOrganizationDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Url]
        public string? Website { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public int? EmployeesCount { get; set; }

        public string? Industry { get; set; }

        public string? Description { get; set; }

        public OrganizationStatus Status { get; set; }
    }

    public class OrganizationDepartmentDto
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }
    }

    public class CreateOrganizationDepartmentDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateOrganizationDepartmentDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}
