using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class OrganizationCheckListAnswersConfiguration : IEntityTypeConfiguration<OrganizationCheckListAnswers>
    {
        public void Configure(EntityTypeBuilder<OrganizationCheckListAnswers> builder)
        {
            builder.ToTable("organization_check_list_answers");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Done)
                .IsRequired();

            builder.Property(a => a.Deleted)
                .IsRequired();

            builder.Property(a => a.CreationDate)
                .IsRequired();

            builder.Property(a => a.CreatedUser)
                .IsRequired();

            builder.Property(a => a.LastModificationDate)
                .IsRequired();

            builder.Property(a => a.LastModificationUser)
                .IsRequired();

            builder.HasOne(a => a.Organization)
                .WithMany(o => o.CheckListAnswers)
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Create unique constraint for organization and check
            builder.HasIndex(a => new { a.OrganizationId, a.CheckId })
                .IsUnique()
                .HasFilter("Deleted = 0");
        }
    }
}