using Application.Interfaces;
using Application.Mappings;
using Core.Common;
using Core.Entities.Identitiy;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Authentication;
using Core.Models.Auth;
using Core.Models.Users;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IAuditService _auditService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IAuditService auditService,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                // Find user by username
                var user = await _unitOfWork.Users.GetUserByUsernameAsync(request.Username);
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
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
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

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                // This would typically validate a reset token sent via email
                // For simplicity, we'll assume the token is valid and identifies the user

                // For a real implementation, you would:
                // 1. Verify the token against stored reset tokens
                // 2. Get the user associated with the token
                // 3. Check if the token is still valid (not expired)

                // Mock implementation - assuming token is the user's ID for demonstration
                long userId;
                if (!long.TryParse(request.Token, out userId))
                {
                    return ApiResponse<bool>.ErrorResponse("Invalid reset token", 400);
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Invalid reset token", 400);
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

                await _auditService.LogActionAsync("Auth", user.Id, "EDIT", "Password reset");

                return ApiResponse<bool>.SuccessResponse(true, "Password reset successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return ApiResponse<bool>.ErrorResponse("Password reset failed due to a server error");
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

                // In a real implementation, you would:
                // 1. Generate a password reset token
                // 2. Save it to the database with an expiration time
                // 3. Send an email to the user with a link containing the token

                // For this implementation, we'll just log the request
                await _auditService.LogActionAsync("Auth", user.Id, "EDIT", "Password reset requested");

                return ApiResponse<bool>.SuccessResponse(true, "If your email exists in our system, you will receive password reset instructions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request for email {Email}", request.Email);
                return ApiResponse<bool>.ErrorResponse("Forgot password request failed due to a server error");
            }
        }
    }
}