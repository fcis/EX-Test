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
                    // Framework permissions
                    Constants.Permissions.ViewFrameworks,
                    Constants.Permissions.CreateFramework,
                    Constants.Permissions.EditFramework,
                    Constants.Permissions.DeleteFramework,
    
                    // Organization permissions
                    Constants.Permissions.ViewOrganizations,
                    Constants.Permissions.CreateOrganization,
                    Constants.Permissions.EditOrganization,
                    Constants.Permissions.DeleteOrganization,
    
                    // User permissions
                    Constants.Permissions.ViewUsers,
                    Constants.Permissions.CreateUser,
                    Constants.Permissions.EditUser,
                    Constants.Permissions.DeleteUser,
    
                    // Audit permissions
                    Constants.Permissions.ViewAudit,
    
                    // Category permissions
                    Constants.Permissions.ViewCategories,
                    Constants.Permissions.CreateCategory,
                    Constants.Permissions.EditCategory,
                    Constants.Permissions.DeleteCategory,
    
                    // Clause permissions
                    Constants.Permissions.ViewClauses,
                    Constants.Permissions.CreateClause,
                    Constants.Permissions.EditClause,
                    Constants.Permissions.DeleteClause
                };

                var permissions = permissionNames.Select(name => new Permission { Name = name }).ToList();

                // Add permissions
                await _context.Permissions.AddRangeAsync(permissions);
                await _context.SaveChangesAsync();

                // Get all roles
                var roles = await _context.Roles.ToListAsync();

                // Define permission mappings for different roles
                var permissionsByRole = new Dictionary<string, List<string>>
                {
                    // Admin gets all permissions
                    [Constants.Roles.Admin] = permissionNames.ToList(),

                    // Organization Admin gets limited permissions
                    [Constants.Roles.Auditor] = new List<string>
                        {
                            Constants.Permissions.ViewFrameworks,
                            Constants.Permissions.ViewOrganizations,
                            Constants.Permissions.EditOrganization,
                            Constants.Permissions.ViewUsers,
                            Constants.Permissions.CreateUser,
                            Constants.Permissions.EditUser
                        },

                    // Organization User gets minimal permissions
                    [Constants.Roles.ComplianceManager] = new List<string>
                        {
                            Constants.Permissions.ViewFrameworks,
                            Constants.Permissions.ViewOrganizations
                        }
                };

                // Get all permissions from the database after saving
                var permissionEntities = await _context.Permissions.ToListAsync();
                var permissionMap = permissionEntities.ToDictionary(p => p.Name, p => p.Id);

                // Assign permissions to roles
                var rolePermissionsToAdd = new List<RolePermissions>();

                foreach (var role in roles)
                {
                    if (permissionsByRole.TryGetValue(role.Name, out var rolePermissions))
                    {
                        foreach (var permName in rolePermissions)
                        {
                            if (permissionMap.TryGetValue(permName, out var permId))
                            {
                                rolePermissionsToAdd.Add(new RolePermissions
                                {
                                    RoleId = role.Id,
                                    PermissionId = permId
                                });
                            }
                        }
                    }
                }

                // Add all role permissions
                if (rolePermissionsToAdd.Any())
                {
                    await _context.RolePermissions.AddRangeAsync(rolePermissionsToAdd);
                    await _context.SaveChangesAsync();
                }

                Console.WriteLine("Permissions seeded and assigned to roles successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while seeding permissions: {ex.Message}");
                throw;
            }
        }
    }
}