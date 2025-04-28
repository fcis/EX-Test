using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("assessment_item_document")]
    public class AssessmentItemDocument : BaseEntity
    {
        public long AssessmentItemId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public long? DepartmentId { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual AssessmentItem AssessmentItem { get; set; } = null!;
        public virtual OrganizationDepartments? Department { get; set; }
    }
}
