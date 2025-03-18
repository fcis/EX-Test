using Core.Entities.Identitiy;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("organization")]
    public class Organization : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? EmployeesCount { get; set; }
        public string? Industry { get; set; }
        public string? Description { get; set; }
        public OrganizationStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual ICollection<OrganizationDepartments> Departments { get; set; } = new List<OrganizationDepartments>();
        public virtual ICollection<OrganizationMembership> Memberships { get; set; } = new List<OrganizationMembership>();
        public virtual ICollection<OrganizationClauseAnswers> ClauseAnswers { get; set; } = new List<OrganizationClauseAnswers>();
        public virtual ICollection<OrganizationCheckListAnswers> CheckListAnswers { get; set; } = new List<OrganizationCheckListAnswers>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
