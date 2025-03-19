// Data/Seeds/RoleSeed.cs
using Core.Common;
using Core.Entities.Identitiy;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Seeds
{
    public class RoleSeed
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleSeed> _logger;

        public RoleSeed(AppDbContext context, ILogger<RoleSeed> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if roles already exist
                if (await _context.Roles.AnyAsync())
                {
                    _logger.LogInformation("Roles already seeded");
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

                _logger.LogInformation("Roles seeded successfully");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding roles");
                throw;
            }
        }
    }
}