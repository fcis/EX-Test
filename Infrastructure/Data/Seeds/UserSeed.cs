using Core.Common;
using Core.Entities.Identitiy;
using Core.Enums;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Data.Seeds
{
    public class UserSeed
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UserSeed(
            AppDbContext context,
            IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if admin user already exists
                if (await _context.Users.AnyAsync(u => u.Username == "admin"))
                {
                    Console.WriteLine("Admin user already exists");
                    return;
                }

                // Get admin role (ensure RoleSeed has run first)
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Constants.Roles.Admin);
                if (adminRole == null)
                {
                    Console.WriteLine("Admin role not found. Make sure RoleSeed has run first.");
                    return;
                }

                // Create admin user
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    Name = "System Administrator",
                    Password = _passwordHasher.HashPassword("Admin@123"), // Change this in production
                    RoleId = adminRole.Id,
                    Status = UserStatus.ACTIVE,
                    CreatedAt = DateTime.UtcNow,
                    LastModificationDate = DateTime.UtcNow
                };
                if (! await _context.Users.AnyAsync(u => u.Username == "Nabarawy"))
                {
                    var SecondUser = new User
                    {
                        Username = "Nabarawy",
                        Email = "tepic41647@birige.com",
                        Name = "System Administrator",
                        Password = _passwordHasher.HashPassword("Admin@1234"), // Change this in production
                        RoleId = adminRole.Id,
                        Status = UserStatus.ACTIVE,
                        CreatedAt = DateTime.UtcNow,
                        LastModificationDate = DateTime.UtcNow
                    };
                    await _context.Users.AddAsync(SecondUser);

                }

                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();

                Console.WriteLine("Admin user seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while seeding admin user: {ex.Message}");
                throw;
            }
        }
    }
}