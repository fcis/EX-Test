using Application.DTOs.Organization;
using Application.Interfaces;
using Application.Mappings;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{


    public class OrganizationService : IOrganizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ICurrentUserService currentUserService,
            ILogger<OrganizationService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedList<OrganizationListDto>>> GetOrganizationsAsync(PagingParameters pagingParameters)
        {
            try
            {
                var organizations = await _unitOfWork.Organizations.GetPagedListAsync(
                    pagingParameters,
                    predicate: o => o.Status != OrganizationStatus.DELETED);

                var organizationDtos = new List<OrganizationListDto>();
                foreach (var org in organizations.Items)
                {
                    var dto = org.ToListDto();
                    dto.DepartmentsCount = await _unitOfWork.OrganizationDepartments.CountAsync(d => d.OrganizationId == org.Id && !d.Deleted);
                    dto.UsersCount = await _unitOfWork.Users.CountAsync(u => u.OrganizationId == org.Id);
                    dto.FrameworksCount = await _unitOfWork.Organizations.CountAsync(o => o.Memberships.Any(m => m.OrganizationId == org.Id && m.Status != OrganizationMembershipStatus.DELETED));
                    organizationDtos.Add(dto);
                }

                var pagedResult = new PagedList<OrganizationListDto>(
                    organizationDtos,
                    organizations.TotalCount,
                    organizations.PageNumber,
                    organizations.PageSize);

                return ApiResponse<PagedList<OrganizationListDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizations");
                return ApiResponse<PagedList<OrganizationListDto>>.ErrorResponse("Failed to retrieve organizations");
            }
        }

        public async Task<ApiResponse<OrganizationDto>> GetOrganizationByIdAsync(long id)
        {
            try
            {
                var organization = await _unitOfWork.Organizations.GetOrganizationWithDepartmentsAsync(id);
                if (organization == null || organization.Status == OrganizationStatus.DELETED)
                {
                    return ApiResponse<OrganizationDto>.ErrorResponse("Organization not found", 404);
                }

                var organizationDto = organization.ToDto();
                return ApiResponse<OrganizationDto>.SuccessResponse(organizationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization with ID {Id}", id);
                return ApiResponse<OrganizationDto>.ErrorResponse("Failed to retrieve organization");
            }
        }

        public async Task<ApiResponse<OrganizationDto>> CreateOrganizationAsync(CreateOrganizationDto createDto)
        {
            try
            {
                // Check if organization with same email exists
                if (!string.IsNullOrEmpty(createDto.Email))
                {
                    var existingByEmail = await _unitOfWork.Organizations.FindSingleWithIncludesAsync(
                        o => o.Email == createDto.Email && o.Status != OrganizationStatus.DELETED);

                    if (existingByEmail != null)
                    {
                        return ApiResponse<OrganizationDto>.ErrorResponse("An organization with this email already exists");
                    }
                }

                // Add check for unique website
                if (!string.IsNullOrEmpty(createDto.Website))
                {
                    var existingByWebsite = await _unitOfWork.Organizations.FindSingleWithIncludesAsync(
                        o => o.Website == createDto.Website && o.Status != OrganizationStatus.DELETED);

                    if (existingByWebsite != null)
                    {
                        return ApiResponse<OrganizationDto>.ErrorResponse("An organization with this website already exists");
                    }
                }

                // Check if organization with same name exists
                var existingByName = await _unitOfWork.Organizations.FindSingleWithIncludesAsync(
                    o => o.Name == createDto.Name && o.Status != OrganizationStatus.DELETED);

                if (existingByName != null)
                {
                    return ApiResponse<OrganizationDto>.ErrorResponse("An organization with this name already exists");
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var organization = createDto.ToEntity();
                    organization.Status = OrganizationStatus.ACTIVE;
                    organization.CreationDate = DateTime.UtcNow;
                    organization.LastModificationDate = DateTime.UtcNow;

                    // Clear departments to add them separately
                    var departments = createDto.Departments;
                    organization.Departments = new List<OrganizationDepartments>();

                    _unitOfWork.Organizations.Add(organization);
                    await _unitOfWork.CompleteAsync();

                    // Add departments
                    var currentUserId = _currentUserService.UserId ?? 0; // Get current user ID

                    foreach (var deptDto in departments)
                    {
                        var department = new OrganizationDepartments
                        {
                            OrganizationId = organization.Id,
                            Name = deptDto.Name,
                            Deleted = false,
                            CreatedUser = currentUserId,
                            LastModificationUser = currentUserId,
                            CreationDate = DateTime.UtcNow,
                            LastModificationDate = DateTime.UtcNow
                        };

                        _unitOfWork.OrganizationDepartments.Add(department);
                    }

                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    await _auditService.LogActionAsync("Organization", organization.Id, "ADD",
                        $"Created organization: {organization.Name}");

                    // Get the organization with departments
                    var createdOrg = await _unitOfWork.Organizations.GetOrganizationWithDepartmentsAsync(organization.Id);
                    var organizationDto = createdOrg.ToDto();

                    return ApiResponse<OrganizationDto>.SuccessResponse(organizationDto, "Organization created successfully");
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization");
                return ApiResponse<OrganizationDto>.ErrorResponse("Failed to create organization");
            }
        }

        public async Task<ApiResponse<OrganizationDto>> UpdateOrganizationAsync(long id, UpdateOrganizationDto updateDto)
        {
            try
            {
                var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
                if (organization == null || organization.Status == OrganizationStatus.DELETED)
                {
                    return ApiResponse<OrganizationDto>.ErrorResponse("Organization not found", 404);
                }

                // Check if organization with same email exists (except current one)
                if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != organization.Email)
                {
                    var existingByEmail = await _unitOfWork.Organizations.FindSingleWithIncludesAsync(
                        o => o.Email == updateDto.Email && o.Id != id && o.Status != OrganizationStatus.DELETED);

                    if (existingByEmail != null)
                    {
                        return ApiResponse<OrganizationDto>.ErrorResponse("An organization with this email already exists");
                    }
                }

                // Check if organization with same name exists (except current one)
                if (updateDto.Name != organization.Name)
                {
                    var existingByName = await _unitOfWork.Organizations.FindSingleWithIncludesAsync(
                        o => o.Name == updateDto.Name && o.Id != id && o.Status != OrganizationStatus.DELETED);

                    if (existingByName != null)
                    {
                        return ApiResponse<OrganizationDto>.ErrorResponse("An organization with this name already exists");
                    }
                }

                // Update organization
                organization.Name = updateDto.Name;
                organization.Website = updateDto.Website;
                organization.Email = updateDto.Email;
                organization.Phone = updateDto.Phone;
                organization.EmployeesCount = updateDto.EmployeesCount;
                organization.Industry = updateDto.Industry;
                organization.Description = updateDto.Description;
                organization.Status = updateDto.Status;
                organization.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Organizations.Update(organization);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Organization", organization.Id, "EDIT",
                    $"Updated organization: {organization.Name}");

                // Get updated organization with departments
                var updatedOrg = await _unitOfWork.Organizations.GetOrganizationWithDepartmentsAsync(id);
                var organizationDto = updatedOrg.ToDto();

                return ApiResponse<OrganizationDto>.SuccessResponse(organizationDto, "Organization updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization with ID {Id}", id);
                return ApiResponse<OrganizationDto>.ErrorResponse("Failed to update organization");
            }
        }

        public async Task<ApiResponse<bool>> DeleteOrganizationAsync(long id)
        {
            try
            {
                var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
                if (organization == null || organization.Status == OrganizationStatus.DELETED)
                {
                    return ApiResponse<bool>.ErrorResponse("Organization not found", 404);
                }

                // Check if the organization has users
                var usersCount = await _unitOfWork.Users.CountAsync(u => u.OrganizationId == id);
                if (usersCount > 0)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete an organization with associated users");
                }

                // Soft delete the organization
                organization.Status = OrganizationStatus.DELETED;
                organization.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Organizations.Update(organization);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Organization", organization.Id, "DELETE",
                    $"Deleted organization: {organization.Name}");

                return ApiResponse<bool>.SuccessResponse(true, "Organization deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organization with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete organization");
            }
        }

        public async Task<ApiResponse<OrganizationDepartmentDto>> AddDepartmentAsync(long organizationId, CreateOrganizationDepartmentDto createDto)
        {
            try
            {
                // Verify organization exists
                var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId);
                if (organization == null || organization.Status == OrganizationStatus.DELETED)
                {
                    return ApiResponse<OrganizationDepartmentDto>.ErrorResponse("Organization not found", 404);
                }

                // Check if department with same name exists for this organization
                var existingDepartment = await _unitOfWork.OrganizationDepartments.FindSingleWithIncludesAsync(
                    d => d.OrganizationId == organizationId && d.Name == createDto.Name && !d.Deleted);

                if (existingDepartment != null)
                {
                    return ApiResponse<OrganizationDepartmentDto>.ErrorResponse("A department with this name already exists for this organization");
                }

                var department = new OrganizationDepartments
                {
                    OrganizationId = organizationId,
                    Name = createDto.Name,
                    Deleted = false,
                    CreationDate = DateTime.UtcNow,
                    LastModificationDate = DateTime.UtcNow
                };

                _unitOfWork.OrganizationDepartments.Add(department);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("OrganizationDepartment", department.Id, "ADD",
                    $"Added department {department.Name} to organization {organization.Name}");

                var departmentDto = department.ToDto();
                return ApiResponse<OrganizationDepartmentDto>.SuccessResponse(departmentDto, "Department added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding department to organization {OrganizationId}", organizationId);
                return ApiResponse<OrganizationDepartmentDto>.ErrorResponse("Failed to add department");
            }
        }

        public async Task<ApiResponse<OrganizationDepartmentDto>> UpdateDepartmentAsync(long id, UpdateOrganizationDepartmentDto updateDto)
        {
            try
            {
                var department = await _unitOfWork.OrganizationDepartments.GetByIdAsync(id);
                if (department == null || department.Deleted)
                {
                    return ApiResponse<OrganizationDepartmentDto>.ErrorResponse("Department not found", 404);
                }

                // Check if department with same name exists (except current one)
                var existingDepartment = await _unitOfWork.OrganizationDepartments.FindSingleWithIncludesAsync(
                    d => d.OrganizationId == department.OrganizationId &&
                         d.Name == updateDto.Name &&
                         d.Id != id &&
                         !d.Deleted);

                if (existingDepartment != null)
                {
                    return ApiResponse<OrganizationDepartmentDto>.ErrorResponse("A department with this name already exists for this organization");
                }

                // Update department
                department.Name = updateDto.Name;
                department.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.OrganizationDepartments.Update(department);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("OrganizationDepartment", department.Id, "EDIT",
                    $"Updated department: {department.Name}");

                var departmentDto = department.ToDto();
                return ApiResponse<OrganizationDepartmentDto>.SuccessResponse(departmentDto, "Department updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department with ID {Id}", id);
                return ApiResponse<OrganizationDepartmentDto>.ErrorResponse("Failed to update department");
            }
        }

        public async Task<ApiResponse<bool>> DeleteDepartmentAsync(long id)
        {
            try
            {
                var department = await _unitOfWork.OrganizationDepartments.GetByIdAsync(id);
                if (department == null || department.Deleted)
                {
                    return ApiResponse<bool>.ErrorResponse("Department not found", 404);
                }

                // Check if the department has users
                var usersCount = await _unitOfWork.Users.CountAsync(u => u.OrganizationDepartmentId == id);
                if (usersCount > 0)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete a department with associated users");
                }

                // Soft delete the department
                department.Deleted = true;
                department.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.OrganizationDepartments.Update(department);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("OrganizationDepartment", department.Id, "DELETE",
                    $"Deleted department: {department.Name}");

                return ApiResponse<bool>.SuccessResponse(true, "Department deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete department");
            }
        }

        public async Task<ApiResponse<bool>> AssignFrameworkAsync(long organizationId, long frameworkId, long frameworkVersionId)
        {
            try
            {
                // Verify organization exists
                var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId);
                if (organization == null || organization.Status == OrganizationStatus.DELETED)
                {
                    return ApiResponse<bool>.ErrorResponse("Organization not found", 404);
                }

                // Verify framework exists and is published
                var framework = await _unitOfWork.Frameworks.GetByIdAsync(frameworkId);
                if (framework == null || framework.Status != FrameworkStatus.ACTIVE)
                {
                    return ApiResponse<bool>.ErrorResponse("Framework not found or not published", 404);
                }

                // Verify framework version exists and belongs to the framework
                var version = await _unitOfWork.FrameworkVersions.GetByIdAsync(frameworkVersionId);
                if (version == null || version.FrameworkId != frameworkId || version.Status != FrameworkVersionStatus.PUBLISED)
                {
                    return ApiResponse<bool>.ErrorResponse("Framework version not found or not published", 404);
                }

                // Check if organization is already assigned to this framework/version
                var existingMembership = await _unitOfWork.Organizations.FindSingleWithIncludesAsync(
                    o => o.Id == organizationId &&
                         o.Memberships.Any(m => m.FrameworkId == frameworkId &&
                                              m.FrameworkVersionId == frameworkVersionId &&
                                              m.Status != OrganizationMembershipStatus.DELETED));

                if (existingMembership != null)
                {
                    return ApiResponse<bool>.ErrorResponse("Organization is already assigned to this framework version");
                }

                // Create membership
                var membership = new OrganizationMembership
                {
                    OrganizationId = organizationId,
                    FrameworkId = frameworkId,
                    FrameworkVersionId = frameworkVersionId,
                    Status = OrganizationMembershipStatus.ACTIVE,
                    CreationDate = DateTime.UtcNow,
                    LastModificationDate = DateTime.UtcNow
                };

                _unitOfWork.OrganizationMembershipRepository.Add(membership);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("OrganizationMembership", membership.Id, "ADD",
                    $"Assigned framework {framework.Name} (version {version.Name}) to organization {organization.Name}");

                return ApiResponse<bool>.SuccessResponse(true, "Framework assigned to organization successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning framework {FrameworkId} to organization {OrganizationId}", frameworkId, organizationId);
                return ApiResponse<bool>.ErrorResponse("Failed to assign framework to organization");
            }
        }
    }
}