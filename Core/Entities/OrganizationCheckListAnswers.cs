using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("organization_check_list_answers")]
    public class OrganizationCheckListAnswers : BaseEntity
    {
        public long OrganizationId { get; set; }
        public long CheckId { get; set; }
        public bool Done { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
    }
}
