using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Data.Seeds
{
    public static class SeedManager
    {
        public static async Task SeedDataAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                Console.WriteLine("Starting data seeding");

                var context = services.GetRequiredService<AppDbContext>();
                var passwordHasher = services.GetRequiredService<IPasswordHasher>();

                // 1. Seed roles first
                var roleSeed = new RoleSeed(context);
                await roleSeed.SeedAsync();

                // 2. Seed permissions and assign to roles
                var permissionSeed = new PermissionSeed(context);
                await permissionSeed.SeedAsync();

                // 3. Seed users last (depends on roles)
                var userSeed = new UserSeed(context, passwordHasher);
                await userSeed.SeedAsync();

                Console.WriteLine("Data seeding completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during data seeding: {ex.Message}");
                throw;
            }
        }
    }
}