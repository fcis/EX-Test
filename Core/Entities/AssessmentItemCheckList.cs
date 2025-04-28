using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("assessment_item_checklist")]
    public class AssessmentItemCheckList : BaseEntity
    {
        public long AssessmentItemId { get; set; }
        public long CheckListId { get; set; }
        public bool IsChecked { get; set; }
        public string? Notes { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual AssessmentItem AssessmentItem { get; set; } = null!;
        public virtual ClauseCheckList CheckList { get; set; } = null!;
    }
}
