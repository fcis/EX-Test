// Data/AppDbContext.cs
using Core.Entities;
using Core.Entities.Identitiy;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Identity
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }

        // Compliance Framework
        public DbSet<Framework> Frameworks { get; set; }
        public DbSet<FrameworkVersion> FrameworkVersions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FrameworkCategories> FrameworkCategories { get; set; }
        public DbSet<Clause> Clauses { get; set; }
        public DbSet<FwCatClauses> FwCatClauses { get; set; }
        public DbSet<ClauseCheckList> ClauseCheckLists { get; set; }

        // Organizations
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationDepartments> OrganizationDepartments { get; set; }
        public DbSet<OrganizationMembership> OrganizationMemberships { get; set; }
        public DbSet<OrganizationClauseAnswers> OrganizationClauseAnswers { get; set; }
        public DbSet<OrganizationCheckListAnswers> OrganizationCheckListAnswers { get; set; }

        // Audit
        public DbSet<Audit> Audits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations from all IEntityTypeConfiguration classes in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}