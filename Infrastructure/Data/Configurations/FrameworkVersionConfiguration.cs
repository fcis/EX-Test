using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class FrameworkVersionConfiguration : IEntityTypeConfiguration<FrameworkVersion>
    {
        public void Configure(EntityTypeBuilder<FrameworkVersion> builder)
        {
            builder.ToTable("framework_version");

            builder.HasKey(fv => fv.Id);

            builder.Property(fv => fv.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(fv => fv.Description)
                .HasMaxLength(500);

            builder.Property(fv => fv.Status)
                .IsRequired();

            builder.Property(fv => fv.VersionDate)
                .IsRequired();

            builder.Property(fv => fv.CreationDate)
                .IsRequired();

            builder.Property(fv => fv.CreatedUser)
                .IsRequired();

            builder.Property(fv => fv.LastModificationDate)
                .IsRequired();

            builder.Property(fv => fv.LastModificationUser)
                .IsRequired();

            builder.HasOne(fv => fv.Framework)
                .WithMany(f => f.Versions)
                .HasForeignKey(fv => fv.FrameworkId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}