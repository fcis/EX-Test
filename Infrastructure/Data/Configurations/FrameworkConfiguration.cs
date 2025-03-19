using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class FrameworkConfiguration : IEntityTypeConfiguration<Framework>
    {
        public void Configure(EntityTypeBuilder<Framework> builder)
        {
            builder.ToTable("framework");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(f => f.Description)
                .HasMaxLength(500);

            builder.Property(f => f.Status)
                .IsRequired();

            builder.Property(f => f.CreationDate)
                .IsRequired();

            builder.Property(f => f.CreatedUser)
                .IsRequired();

            builder.Property(f => f.LastModificationDate)
                .IsRequired();

            builder.Property(f => f.LastModificationUser)
                .IsRequired();
        }
    }
}