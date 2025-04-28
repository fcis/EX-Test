using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("assessment_item")]
    public class AssessmentItem : BaseEntity
    {
        public long AssessmentId { get; set; }
        public long ClauseId { get; set; }
        public ComplianceStatus Status { get; set; }
        public string? Notes { get; set; }
        public string? CorrectiveActions { get; set; }
        public long? AssignedDepartmentId { get; set; }
        public DateTime? DueDate { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual Assessment Assessment { get; set; } = null!;
        public virtual Clause Clause { get; set; } = null!;
        public virtual OrganizationDepartments? AssignedDepartment { get; set; }
        public virtual ICollection<AssessmentItemDocument> Documents { get; set; } = new List<AssessmentItemDocument>();
        public virtual ICollection<AssessmentItemCheckList> CheckListItems { get; set; } = new List<AssessmentItemCheckList>();
    }
}
