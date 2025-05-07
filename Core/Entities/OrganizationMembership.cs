using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("organization_membership")]
    public class OrganizationMembership : BaseEntity
    {
        public long OrganizationId { get; set; }
        public long FrameworkId { get; set; }
        public long FrameworkVersionId { get; set; }
        public OrganizationMembershipStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }

        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
        public virtual Framework Framework { get; set; } = null!;
        public virtual FrameworkVersion FrameworkVersion { get; set; } = null!;

    }
}
