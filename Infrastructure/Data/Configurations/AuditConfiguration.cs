// Data/Configurations/AuditConfiguration.cs
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class AuditConfiguration : IEntityTypeConfiguration<Audit>
    {
        public void Configure(EntityTypeBuilder<Audit> builder)
        {
            builder.ToTable("audit");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Entity)
                .HasMaxLength(100);

            builder.Property(a => a.Ip)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(a => a.When)
                .IsRequired();

            builder.Property(a => a.Action)
                .IsRequired();
        }
    }
}