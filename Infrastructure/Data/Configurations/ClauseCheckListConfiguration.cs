using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ClauseCheckListConfiguration : IEntityTypeConfiguration<ClauseCheckList>
    {
        public void Configure(EntityTypeBuilder<ClauseCheckList> builder)
        {
            builder.ToTable("clause_check_list");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(200)
                .IsRequired();

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

            builder.HasOne(c => c.Clause)
                .WithMany(cl => cl.CheckLists)
                .HasForeignKey(c => c.ClauseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}