// Authentication/PasswordHasher.cs
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly ILogger<PasswordHasher> _logger;

        public PasswordHasher(ILogger<PasswordHasher> logger)
        {
            _logger = logger;
        }

        public string HashPassword(string password)
        {
            try
            {
                return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw new ApplicationException("Password hashing failed", ex);
            }
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }
    }
}