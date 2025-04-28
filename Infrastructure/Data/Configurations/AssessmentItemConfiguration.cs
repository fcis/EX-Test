using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class AssessmentItemConfiguration : IEntityTypeConfiguration<AssessmentItem>
    {
        public void Configure(EntityTypeBuilder<AssessmentItem> builder)
        {
            builder.ToTable("assessment_item");

            builder.HasKey(ai => ai.Id);

            builder.Property(ai => ai.Notes)
                .HasMaxLength(500);

            builder.Property(ai => ai.CorrectiveActions)
                .HasMaxLength(500);

            builder.Property(ai => ai.Status)
                .IsRequired();

            builder.Property(ai => ai.Deleted)
                .IsRequired();

            builder.Property(ai => ai.CreationDate)
                .IsRequired();

            builder.Property(ai => ai.CreatedUser)
                .IsRequired();

            builder.Property(ai => ai.LastModificationDate)
                .IsRequired();

            builder.Property(ai => ai.LastModificationUser)
                .IsRequired();

            // Define relationships
            builder.HasOne(ai => ai.Assessment)
                .WithMany(a => a.AssessmentItems)
                .HasForeignKey(ai => ai.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ai => ai.Clause)
                .WithMany()
                .HasForeignKey(ai => ai.ClauseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ai => ai.AssignedDepartment)
                .WithMany()
                .HasForeignKey(ai => ai.AssignedDepartmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}