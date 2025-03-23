using Application.Interfaces;
using Application.Mappings;
using Core.Common;
using Core.Entities.Identitiy;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Authentication;
using Core.Models.Auth;
using Core.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly ICurrentTokenProvider _currentTokenProvider;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IAuditService auditService,
            IEmailService emailService,
            ITokenBlacklistService tokenBlacklistService,
            ICurrentTokenProvider currentTokenProvider,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _auditService = auditService;
            _emailService = emailService;
            _tokenBlacklistService = tokenBlacklistService;
            _currentTokenProvider = currentTokenProvider;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                // Find user by username
                var user = await _unitOfWork.Users.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse("Invalid username or password", 401);
                }

                // Check if user is active
                if (user.Status != UserStatus.ACTIVE && user.Status != UserStatus.NEW)
                {
                    if (user.Status == UserStatus.LOCKED)
                    {
                        return ApiResponse<AuthResponse>.ErrorResponse("Account is locked. Please contact an administrator.", 401);
                    }
                    else if (user.Status == UserStatus.BLOCKED)
                    {
                        return ApiResponse<AuthResponse>.ErrorResponse("Account is blocked. Please contact an administrator.", 401);
                    }
                    else
                    {
                        return ApiResponse<AuthResponse>.ErrorResponse("Account is not active. Please contact an administrator.", 401);
                    }
                }

                // Verify password
                if (!_passwordHasher.VerifyPassword(user.Password, request.Password))
                {
                    // Increment wrong password count
                    user.WrongPasswordCount++;

                    // Lock account if too many failed attempts
                    if (user.WrongPasswordCount >= 3)
                    {
                        user.Status = UserStatus.LOCKED;
                    }

                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.CompleteAsync();

                    return ApiResponse<AuthResponse>.ErrorResponse("Invalid username or password", 401);
                }

                // Reset wrong password count
                user.WrongPasswordCount = 0;

                // Activate the user if it's the first login
                if (user.Status == UserStatus.NEW)
                {
                    user.Status = UserStatus.ACTIVE;
                }

                // Load role and permissions
                var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(user.RoleId);
                var permissions = role.Permissions.Select(p => p.Permission.Name).ToList();

                // Generate tokens
                var token = _jwtTokenGenerator.GenerateToken(user, new List<string> { role.Name }, permissions);
                var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

                // Update user with refresh token
                user.Token = refreshToken;
                user.TokenValidity = DateTime.UtcNow.AddDays(7);
                user.LastLogin = DateTime.UtcNow;
                user.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Auth", user.Id, "LOGON", $"User logged in: {user.Username}");

                // Create response
                var response = new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    TokenExpiration = DateTime.UtcNow.AddHours(1), // Assuming token expires in 1 hour
                    UserId = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Email = user.Email,
                    Role = role.Name,
                    Permissions = permissions,
                    OrganizationId = user.OrganizationId,
                    OrganizationName = user.Organization?.Name
                };

                return ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Email);
                return ApiResponse<AuthResponse>.ErrorResponse("Login failed due to a server error");
            }
        }

        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                // Validate existing token
                var userId = _jwtTokenGenerator.ValidateTokenAndGetUserId(request.Token);
                if (!userId.HasValue)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse("Invalid token", 401);
                }

                // Get user
                var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse("User not found", 404);
                }

                // Validate refresh token
                if (user.Token != request.RefreshToken || !user.TokenValidity.HasValue || user.TokenValidity.Value < DateTime.UtcNow)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse("Invalid refresh token", 401);
                }

                // Check if user is active
                if (user.Status != UserStatus.ACTIVE)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse("Account is not active", 401);
                }

                // Load role and permissions
                var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(user.RoleId);
                var permissions = role.Permissions.Select(p => p.Permission.Name).ToList();

                // Generate new tokens
                var newToken = _jwtTokenGenerator.GenerateToken(user, new List<string> { role.Name }, permissions);
                var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

                // Update user with new refresh token
                user.Token = newRefreshToken;
                user.TokenValidity = DateTime.UtcNow.AddDays(7);
                user.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                // Create response
                var response = new AuthResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    TokenExpiration = DateTime.UtcNow.AddHours(1), // Assuming token expires in 1 hour
                    UserId = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Email = user.Email,
                    Role = role.Name,
                    Permissions = permissions,
                    OrganizationId = user.OrganizationId,
                    OrganizationName = user.Organization?.Name
                };

                return ApiResponse<AuthResponse>.SuccessResponse(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return ApiResponse<AuthResponse>.ErrorResponse("Token refresh failed due to a server error");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string username)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found", 404);
                }

                // Extract the JWT token from the current request
                var currentToken = _currentTokenProvider.GetCurrentToken();

                if (!string.IsNullOrEmpty(currentToken))
                {
                    // Get token expiration from JWT
                    var tokenExpiration = _jwtTokenGenerator.GetTokenExpiration(currentToken);

                    // Add the current token to the blacklist
                    await _tokenBlacklistService.AddToBlacklistAsync(currentToken, tokenExpiration);
                }

                // Invalidate refresh token
                user.Token = null;
                user.TokenValidity = null;
                user.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Auth", user.Id, "LOGOUT", $"User logged out: {user.Username}");

                return ApiResponse<bool>.SuccessResponse(true, "Logout successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {Username}", username);
                return ApiResponse<bool>.ErrorResponse("Logout failed due to a server error");
            }
        }
        public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserByUsernameAsync(request.Username);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found", 404);
                }

                // Verify current password
                if (!_passwordHasher.VerifyPassword(user.Password, request.CurrentPassword))
                {
                    return ApiResponse<bool>.ErrorResponse("Current password is incorrect", 400);
                }

                // Update password
                user.Password = _passwordHasher.HashPassword(request.NewPassword);
                user.LastModificationDate = DateTime.UtcNow;

                // Reset wrong password count
                user.WrongPasswordCount = 0;

                // Unlock account if it was locked
                if (user.Status == UserStatus.LOCKED)
                {
                    user.Status = UserStatus.ACTIVE;
                }

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Auth", user.Id, "EDIT", "Password changed");

                return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {Username}", request.Username);
                return ApiResponse<bool>.ErrorResponse("Password change failed due to a server error");
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    // For security reasons, still return success even if email doesn't exist
                    return ApiResponse<bool>.SuccessResponse(true, "If your email exists in our system, you will receive password reset instructions");
                }

                // Generate a reset token
                var resetToken = GenerateSecureToken();

                // Store the reset token with an expiration time (24 hours)
                user.Token = resetToken;
                user.TokenValidity = DateTime.UtcNow.AddHours(24);

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                // Generate a reset link
                var resetLink = $"https://localhost:7229/Auth/reset-password?token={resetToken}";

                // Send email with reset link
                var subject = "Password Reset Request";
                var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hello {user.Name},</p>
                    <p>We received a request to reset your password. If you didn't make this request, you can safely ignore this email.</p>
                    <p>To reset your password, click on the link below:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>
                    <p>This link will expire in 24 hours.</p>
                    <p>Thank you,</p>
                    <p>The Support Team</p>
                </body>
                </html>";

                var emailSent = await _emailService.SendAsync("m.gamal.nabarawy@gmail.com",user.Email, subject, body);

                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send password reset email to {Email}", user.Email);
                    // Don't fail the operation but log the issue
                }

                await _auditService.LogActionAsync("Auth", user.Id, "EDIT", "Password reset requested");

                return ApiResponse<bool>.SuccessResponse(true, "If your email exists in our system, you will receive password reset instructions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request for email {Email}", request.Email);
                return ApiResponse<bool>.ErrorResponse("Forgot password request failed due to a server error");
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                // Find the user by the token
                var user = await _unitOfWork.Users.FindSingleWithIncludesAsync(
                    u => u.Token == request.Token &&
                         u.TokenValidity.HasValue &&
                         u.TokenValidity.Value > DateTime.UtcNow);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Invalid or expired reset token", 400);
                }

                // Update password
                user.Password = _passwordHasher.HashPassword(request.NewPassword);

                // Clear the token after successful reset
                user.Token = null;
                user.TokenValidity = null;

                // Update modified timestamp
                user.LastModificationDate = DateTime.UtcNow;

                // Reset wrong password count
                user.WrongPasswordCount = 0;

                // Unlock account if it was locked
                if (user.Status == UserStatus.LOCKED)
                {
                    user.Status = UserStatus.ACTIVE;
                }

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Auth", user.Id, "EDIT", "Password reset completed");

                // Send password reset confirmation email
                var subject = "Password Reset Confirmation";
                var body = $@"
                <html>
                <body>
                    <h2>Password Reset Confirmation</h2>
                    <p>Hello {user.Name},</p>
                    <p>Your password has been successfully reset.</p>
                    <p>If you did not request this change, please contact support immediately.</p>
                    <p>Thank you,</p>
                    <p>The Support Team</p>
                </body>
                </html>";

                await _emailService.SendAsync("",user.Email, subject, body);

                return ApiResponse<bool>.SuccessResponse(true, "Password reset successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return ApiResponse<bool>.ErrorResponse("Password reset failed due to a server error");
            }
        }

        // Helper method to generate a secure token
        private string GenerateSecureToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}