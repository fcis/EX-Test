using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("assessment")]
    public class Assessment : BaseEntity
    {
        public long OrganizationMembershipId { get; set; } // New field
        public AssessmentStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Notes { get; set; }
        public bool Deleted { get; set; }
        public long StartedUser { get; set; } // Changed from CreatedUser
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual OrganizationMembership OrganizationMembership { get; set; } = null!;
        public virtual ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
    }
}
