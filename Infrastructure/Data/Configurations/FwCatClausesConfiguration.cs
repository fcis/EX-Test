using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class FwCatClausesConfiguration : IEntityTypeConfiguration<FwCatClauses>
    {
        public void Configure(EntityTypeBuilder<FwCatClauses> builder)
        {
            builder.ToTable("fw_cat_clauses");

            builder.HasKey(fc => fc.Id);

            builder.Property(fc => fc.ClauseNumber)
                .HasMaxLength(20);

            builder.Property(fc => fc.Deleted)
                .IsRequired();

            builder.Property(fc => fc.CreationDate)
                .IsRequired();

            builder.Property(fc => fc.CreatedUser)
                .IsRequired();

            builder.Property(fc => fc.LastModificationDate)
                .IsRequired();

            builder.Property(fc => fc.LastModificationUser)
                .IsRequired();

            builder.HasOne(fc => fc.FrameworkCategory)
                .WithMany(c => c.Clauses)
                .HasForeignKey(fc => fc.FrameworkCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fc => fc.Clause)
                .WithMany(c => c.FrameworkCategories)
                .HasForeignKey(fc => fc.ClauseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}