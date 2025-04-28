using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class AssessmentItemDocumentConfiguration : IEntityTypeConfiguration<AssessmentItemDocument>
    {
        public void Configure(EntityTypeBuilder<AssessmentItemDocument> builder)
        {
            builder.ToTable("assessment_item_document");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.FileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(d => d.StoragePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(d => d.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.FileSize)
                .IsRequired();

            builder.Property(d => d.UploadDate)
                .IsRequired();

            builder.Property(d => d.DocumentType)
                .HasMaxLength(100);

            builder.Property(d => d.Deleted)
                .IsRequired();

            builder.Property(d => d.CreationDate)
                .IsRequired();

            builder.Property(d => d.CreatedUser)
                .IsRequired();

            builder.Property(d => d.LastModificationDate)
                .IsRequired();

            builder.Property(d => d.LastModificationUser)
                .IsRequired();

            // Define relationships
            builder.HasOne(d => d.AssessmentItem)
                .WithMany(ai => ai.Documents)
                .HasForeignKey(d => d.AssessmentItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Department)
                .WithMany()
                .HasForeignKey(d => d.DepartmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}