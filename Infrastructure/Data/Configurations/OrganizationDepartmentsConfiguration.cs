using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class OrganizationDepartmentsConfiguration : IEntityTypeConfiguration<OrganizationDepartments>
    {
        public void Configure(EntityTypeBuilder<OrganizationDepartments> builder)
        {
            builder.ToTable("organization_departments");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .HasMaxLength(100)
                .IsRequired();

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

            builder.HasOne(d => d.Organization)
                .WithMany(o => o.Departments)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}