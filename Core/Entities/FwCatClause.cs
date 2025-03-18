using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("fw_cat_clauses")]
    public class FwCatClauses : BaseEntity
    {
        public long FrameworkCategoryId { get; set; }
        public long ClauseId { get; set; }
        public string? ClauseNumber { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual FrameworkCategories FrameworkCategory { get; set; } = null!;
        public virtual Clause Clause { get; set; } = null!;
    }
}
