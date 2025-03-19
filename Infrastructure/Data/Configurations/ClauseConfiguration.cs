using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ClauseConfiguration : IEntityTypeConfiguration<Clause>
    {
        public void Configure(EntityTypeBuilder<Clause> builder)
        {
            builder.ToTable("clause");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.Deleted)
                .IsRequired();

            builder.Property(c => c.CreationDate)
                .IsRequired();

            builder.Property(c => c.CreatedUser)
                .IsRequired();

            builder.Property(c => c.LastModificationDate)
                .IsRequired();

            builder.Property(c => c.LastModificationUser)
                .IsRequired();
        }
    }
}