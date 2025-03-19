using Core.Common;
using Core.Entities.Identitiy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Seeds
{
    public class PermissionSeed
    {
        private readonly AppDbContext _context;

        public PermissionSeed(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if permissions already exist
                if (await _context.Permissions.AnyAsync())
                {
                    Console.WriteLine("Permissions already seeded");
                    return;
                }

                // Define permissions based on Constants
                var permissionNames = new List<string>
                {
                    Constants.Permissions.ViewFrameworks,
                    Constants.Permissions.CreateFramework,
                    Constants.Permissions.EditFramework,
                    Constants.Permissions.DeleteFramework,
                    Constants.Permissions.ViewOrganizations,
                    Constants.Permissions.CreateOrganization,
                    Constants.Permissions.EditOrganization,
                    Constants.Permissions.DeleteOrganization,
                    Constants.Permissions.ViewUsers,
                    Constants.Permissions.CreateUser,
                    Constants.Permissions.EditUser,
                    Constants.Permissions.DeleteUser,
                    Constants.Permissions.ViewAudit
                };

                var permissions = permissionNames.Select(name => new Permission { Name = name }).ToList();

                // Add permissions
                await _context.Permissions.AddRangeAsync(permissions);
                await _context.SaveChangesAsync();

                // Get admin role
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Constants.Roles.Admin);
                if (adminRole == null)
                {
                    Console.WriteLine("Admin role not found. Make sure RoleSeed has run first.");
                    return;
                }

                // Assign all permissions to admin role
                var permissionEntities = await _context.Permissions.ToListAsync();
                var rolePermissions = permissionEntities.Select(p => new RolePermissions
                {
                    RoleId = adminRole.Id,
                    PermissionId = p.Id
                }).ToList();

                await _context.RolePermissions.AddRangeAsync(rolePermissions);
                await _context.SaveChangesAsync();

                Console.WriteLine("Permissions seeded and assigned to admin role successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while seeding permissions: {ex.Message}");
                throw;
            }
        }
    }
}