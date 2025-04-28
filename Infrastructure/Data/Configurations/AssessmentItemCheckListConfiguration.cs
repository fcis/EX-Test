using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class AssessmentItemCheckListConfiguration : IEntityTypeConfiguration<AssessmentItemCheckList>
    {
        public void Configure(EntityTypeBuilder<AssessmentItemCheckList> builder)
        {
            builder.ToTable("assessment_item_checklist");

            builder.HasKey(cl => cl.Id);

            builder.Property(cl => cl.Notes)
                .HasMaxLength(500);

            builder.Property(cl => cl.IsChecked)
                .IsRequired();

            builder.Property(cl => cl.Deleted)
                .IsRequired();

            builder.Property(cl => cl.CreationDate)
                .IsRequired();

            builder.Property(cl => cl.CreatedUser)
                .IsRequired();

            builder.Property(cl => cl.LastModificationDate)
                .IsRequired();

            builder.Property(cl => cl.LastModificationUser)
                .IsRequired();

            // Define relationships
            builder.HasOne(cl => cl.AssessmentItem)
                .WithMany(ai => ai.CheckListItems)
                .HasForeignKey(cl => cl.AssessmentItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cl => cl.CheckList)
                .WithMany()
                .HasForeignKey(cl => cl.CheckListId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}