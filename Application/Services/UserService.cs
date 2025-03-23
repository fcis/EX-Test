using Application.DTOs.User;
using Application.Interfaces;
using Application.Mappings;
using Core.Common;
using Core.Entities.Identitiy;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Authentication;
using Core.Models.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// Implementation of the User Service
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuditService _auditService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IAuditService auditService,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(long id)
        {
            try
            {
                var user = await _unitOfWork.Users.FindSingleWithIncludesAsync(
                    u => u.Id == id && u.Status != UserStatus.DELETED,
                    includes: new List<System.Linq.Expressions.Expression<Func<User, object>>>
                    {
                        u => u.Role
                    });

                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found", 404);
                }

                var userDto = user.ToDto();
                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with ID {Id}", id);
                return ApiResponse<UserDto>.ErrorResponse("Failed to retrieve user");
            }
        }

        /// <summary>
        /// Gets paged list of users
        /// </summary>
        public async Task<ApiResponse<PagedList<UserListDto>>> GetUsersAsync(PagingParameters pagingParameters)
        {
            try
            {
                var users = await _unitOfWork.Users.GetPagedUsersAsync(pagingParameters);

                // Map entities to DTOs to avoid circular references
                var userDtos = users.Items.Select(u => u.ToListDto()).ToList();

                // Create new paged list with DTOs
                var pagedDtos = new PagedList<UserListDto>(
                    userDtos,
                    users.TotalCount,
                    users.PageNumber,
                    users.PageSize);

                return ApiResponse<PagedList<UserListDto>>.SuccessResponse(pagedDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return ApiResponse<PagedList<UserListDto>>.ErrorResponse("Failed to retrieve users");
            }
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                // Check if username already exists
                var existingUsername = await _unitOfWork.Users.GetUserByUsernameAsync(request.Username);
                if (existingUsername != null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Username already exists");
                }

                // Check if email already exists
                var existingEmail = await _unitOfWork.Users.GetUserByEmailAsync(request.Email);
                if (existingEmail != null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Email already exists");
                }

                // Verify role exists
                var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
                if (role == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Role not found", 404);
                }

                // Verify organization if specified
                if (request.OrganizationId.HasValue)
                {
                    var organization = await _unitOfWork.Organizations.GetByIdAsync(request.OrganizationId.Value);
                    if (organization == null || organization.Status == OrganizationStatus.DELETED)
                    {
                        return ApiResponse<UserDto>.ErrorResponse("Organization not found", 404);
                    }

                    // Verify department if specified
                    if (request.OrganizationDepartmentId.HasValue)
                    {
                        var department = await _unitOfWork.OrganizationDepartments.GetByIdAsync(request.OrganizationDepartmentId.Value);
                        if (department == null || department.Deleted || department.OrganizationId != request.OrganizationId.Value)
                        {
                            return ApiResponse<UserDto>.ErrorResponse("Department not found or does not belong to the specified organization", 404);
                        }
                    }
                }
                else if (request.OrganizationDepartmentId.HasValue)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Cannot specify a department without an organization");
                }

                // Create user
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    RoleId = request.RoleId,
                    OrganizationId = request.OrganizationId,
                    OrganizationDepartmentId = request.OrganizationDepartmentId,
                    Name = request.Name,
                    Password = _passwordHasher.HashPassword(request.Password),
                    Status = UserStatus.NEW,
                    CreatedAt = DateTime.UtcNow,
                    LastModificationDate = DateTime.UtcNow
                };

                _unitOfWork.Users.Add(user);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("User", user.Id, "ADD", $"Created user: {user.Username}");

                // Fetch the user with relations to create a proper DTO
                var createdUser = await _unitOfWork.Users.FindSingleWithIncludesAsync(
                    u => u.Id == user.Id,
                    includes: new List<System.Linq.Expressions.Expression<Func<User, object>>>
                    {
                        u => u.Role
                    });

                var userDto = createdUser.ToDto();
                return ApiResponse<UserDto>.SuccessResponse(userDto, "User created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return ApiResponse<UserDto>.ErrorResponse("Failed to create user");
            }
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        public async Task<ApiResponse<UserDto>> UpdateUserAsync(long id, UpdateUserRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null || user.Status == UserStatus.DELETED)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found", 404);
                }

                // Check if email already exists (except current user)
                if (user.Email != request.Email)
                {
                    var existingEmail = await _unitOfWork.Users.GetUserByEmailAsync(request.Email);
                    if (existingEmail != null && existingEmail.Id != id)
                    {
                        return ApiResponse<UserDto>.ErrorResponse("Email already exists");
                    }
                }

                // Verify role exists
                var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
                if (role == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Role not found", 404);
                }

                // Verify organization if specified
                if (request.OrganizationId.HasValue)
                {
                    var organization = await _unitOfWork.Organizations.GetByIdAsync(request.OrganizationId.Value);
                    if (organization == null || organization.Status == OrganizationStatus.DELETED)
                    {
                        return ApiResponse<UserDto>.ErrorResponse("Organization not found", 404);
                    }

                    // Verify department if specified
                    if (request.OrganizationDepartmentId.HasValue)
                    {
                        var department = await _unitOfWork.OrganizationDepartments.GetByIdAsync(request.OrganizationDepartmentId.Value);
                        if (department == null || department.Deleted || department.OrganizationId != request.OrganizationId.Value)
                        {
                            return ApiResponse<UserDto>.ErrorResponse("Department not found or does not belong to the specified organization", 404);
                        }
                    }
                }
                else if (request.OrganizationDepartmentId.HasValue)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Cannot specify a department without an organization");
                }

                // Update user
                user.Email = request.Email;
                user.RoleId = request.RoleId;
                user.OrganizationId = request.OrganizationId;
                user.OrganizationDepartmentId = request.OrganizationDepartmentId;
                user.Name = request.Name;
                user.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("User", user.Id, "EDIT", $"Updated user: {user.Username}");

                // Fetch the updated user with relations to create a proper DTO
                var updatedUser = await _unitOfWork.Users.FindSingleWithIncludesAsync(
                    u => u.Id == user.Id,
                    includes: new List<System.Linq.Expressions.Expression<Func<User, object>>>
                    {
                        u => u.Role
                    });

                var userDto = updatedUser.ToDto();
                return ApiResponse<UserDto>.SuccessResponse(userDto, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {Id}", id);
                return ApiResponse<UserDto>.ErrorResponse("Failed to update user");
            }
        }

        /// <summary>
        /// Deletes a user (soft delete)
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteUserAsync(long id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null || user.Status == UserStatus.DELETED)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found", 404);
                }

                // Soft delete the user
                user.Status = UserStatus.DELETED;
                user.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("User", user.Id, "DELETE", $"Deleted user: {user.Username}");

                return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete user");
            }
        }

        /// <summary>
        /// Activates a user
        /// </summary>
        public async Task<ApiResponse<bool>> ActivateUserAsync(long id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null || user.Status == UserStatus.DELETED)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found", 404);
                }

                if (user.Status == UserStatus.ACTIVE)
                {
                    return ApiResponse<bool>.ErrorResponse("User is already active");
                }

                // Activate the user
                user.Status = UserStatus.ACTIVE;
                user.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("User", user.Id, "EDIT", $"Activated user: {user.Username}");

                return ApiResponse<bool>.SuccessResponse(true, "User activated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to activate user");
            }
        }

        /// <summary>
        /// Deactivates a user
        /// </summary>
        public async Task<ApiResponse<bool>> DeactivateUserAsync(long id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null || user.Status == UserStatus.DELETED)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found", 404);
                }

                if (user.Status == UserStatus.BLOCKED)
                {
                    return ApiResponse<bool>.ErrorResponse("User is already deactivated");
                }

                // Deactivate the user
                user.Status = UserStatus.BLOCKED;
                user.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("User", user.Id, "EDIT", $"Deactivated user: {user.Username}");

                return ApiResponse<bool>.SuccessResponse(true, "User deactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to deactivate user");
            }
        }

        /// <summary>
        /// Gets users by organization with pagination
        /// </summary>
        public async Task<ApiResponse<PagedList<UserListDto>>> GetUsersByOrganizationAsync(long organizationId, PagingParameters pagingParameters)
        {
            try
            {
                // Verify organization exists
                var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId);
                if (organization == null || organization.Status == OrganizationStatus.DELETED)
                {
                    return ApiResponse<PagedList<UserListDto>>.ErrorResponse("Organization not found", 404);
                }

                var users = await _unitOfWork.Users.GetPagedListAsync(
                    pagingParameters,
                    predicate: u => u.OrganizationId == organizationId && u.Status != UserStatus.DELETED,
                    includes: new List<System.Linq.Expressions.Expression<Func<User, object>>>
                    {
                        u => u.Role,
                        u => u.OrganizationDepartment
                    });

                // Map entities to DTOs to avoid circular references
                var userDtos = users.Items.Select(u => u.ToListDto()).ToList();

                // Create new paged list with DTOs
                var pagedDtos = new PagedList<UserListDto>(
                    userDtos,
                    users.TotalCount,
                    users.PageNumber,
                    users.PageSize);

                return ApiResponse<PagedList<UserListDto>>.SuccessResponse(pagedDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for organization with ID {OrganizationId}", organizationId);
                return ApiResponse<PagedList<UserListDto>>.ErrorResponse("Failed to retrieve users");
            }
        }
    }
}