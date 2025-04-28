using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
    {
        public void Configure(EntityTypeBuilder<Assessment> builder)
        {
            builder.ToTable("assessment");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Notes)
                .HasMaxLength(500);

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.StartDate)
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

            // Define relationships
            builder.HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.FrameworkVersion)
                .WithMany()
                .HasForeignKey(a => a.FrameworkVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}