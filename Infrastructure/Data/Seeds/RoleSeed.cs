using Core.Common;
using Core.Entities.Identitiy;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data.Seeds
{
    public class RoleSeed
    {
        private readonly AppDbContext _context;

        public RoleSeed(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if roles already exist
                if (await _context.Roles.AnyAsync())
                {
                    Console.WriteLine("Roles already seeded");
                    return;
                }

                // Define roles
                var roles = new List<Role>
                {
                    new Role
                    {
                        Name = Constants.Roles.Admin,
                        Portal = Portal.ADMIN
                    },
                    new Role
                    {
                        Name = Constants.Roles.OrganizationAdmin,
                        Portal = Portal.ORGANIZATION
                    },
                    new Role
                    {
                        Name = Constants.Roles.OrganizationUser,
                        Portal = Portal.ORGANIZATION
                    }
                };

                // Add roles
                await _context.Roles.AddRangeAsync(roles);
                await _context.SaveChangesAsync();

                Console.WriteLine("Roles seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while seeding roles: {ex.Message}");
                throw;
            }
        }
    }
}