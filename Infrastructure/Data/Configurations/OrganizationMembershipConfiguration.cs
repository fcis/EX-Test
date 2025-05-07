using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class OrganizationMembershipConfiguration : IEntityTypeConfiguration<OrganizationMembership>
    {
        public void Configure(EntityTypeBuilder<OrganizationMembership> builder)
        {
            builder.ToTable("organization_membership");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Status)
                .IsRequired();

            builder.Property(m => m.CreationDate)
                .IsRequired();

            builder.Property(m => m.CreatedUser)
                .IsRequired();

            builder.Property(m => m.LastModificationDate)
                .IsRequired();

            builder.Property(m => m.LastModificationUser)
                .IsRequired();

            builder.HasOne(m => m.Organization)
                .WithMany(o => o.Memberships)
                .HasForeignKey(m => m.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Framework)
                .WithMany()
                .HasForeignKey(m => m.FrameworkId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.FrameworkVersion)
                .WithMany()
                .HasForeignKey(m => m.FrameworkVersionId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}