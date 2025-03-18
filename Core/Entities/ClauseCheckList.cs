using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("clause_check_list")]
    public class ClauseCheckList : BaseEntity
    {
        public long ClauseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual Clause Clause { get; set; } = null!;
        public virtual ICollection<OrganizationCheckListAnswers> OrganizationAnswers { get; set; } = new List<OrganizationCheckListAnswers>();
    }
}
