using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class OrganizationClauseAnswersConfiguration : IEntityTypeConfiguration<OrganizationClauseAnswers>
    {
        public void Configure(EntityTypeBuilder<OrganizationClauseAnswers> builder)
        {
            builder.ToTable("organization_clause_answers");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.Deleted)
                .IsRequired();

            builder.Property(a => a.CreationDate)
                .IsRequired();

            builder.Property(a => a.CreatedUser)
                .IsRequired();

            builder.Property(a => a.LastModificationDate)
                .IsRequired();

            builder.Property(a => a.LastModificationUser)
                .IsRequired();

            builder.HasOne(a => a.Organization)
                .WithMany(o => o.ClauseAnswers)
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Clause)
                .WithMany()
                .HasForeignKey(a => a.ClauseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Create unique constraint for organization and clause
            builder.HasIndex(a => new { a.OrganizationId, a.ClauseId })
                .IsUnique()
                .HasFilter("Deleted = 0");
        }
    }
}