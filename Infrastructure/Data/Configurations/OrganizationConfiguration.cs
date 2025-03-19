using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("organization");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(o => o.Website)
                .HasMaxLength(200);

            builder.Property(o => o.Email)
                .HasMaxLength(100);

            builder.Property(o => o.Phone)
                .HasMaxLength(20);

            builder.Property(o => o.Industry)
                .HasMaxLength(100);

            builder.Property(o => o.Description)
                .HasMaxLength(500);

            builder.Property(o => o.Status)
                .IsRequired();

            builder.Property(o => o.CreationDate)
                .IsRequired();

            builder.Property(o => o.CreatedUser)
                .IsRequired();

            builder.Property(o => o.LastModificationDate)
                .IsRequired();

            builder.Property(o => o.LastModificationUser)
                .IsRequired();
        }
    }
}