using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class FrameworkCategoriesConfiguration : IEntityTypeConfiguration<FrameworkCategories>
    {
        public void Configure(EntityTypeBuilder<FrameworkCategories> builder)
        {
            builder.ToTable("framework_categories");

            builder.HasKey(fc => fc.Id);

            builder.Property(fc => fc.CategoryNumber)
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

            builder.HasOne(fc => fc.FrameworkVersion)
                .WithMany(fv => fv.Categories)
                .HasForeignKey(fc => fc.FrameworkVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fc => fc.Category)
                .WithMany(c => c.Frameworks)
                .HasForeignKey(fc => fc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}