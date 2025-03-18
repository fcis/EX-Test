using Application.DTOs.Framework;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{


    public class FrameworkService : IFrameworkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ILogger<FrameworkService> _logger;

        public FrameworkService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ILogger<FrameworkService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedList<FrameworkListDto>>> GetFrameworksAsync(PagingParameters pagingParameters)
        {
            try
            {
                var frameworks = await _unitOfWork.Frameworks.GetPagedListAsync(
                    pagingParameters,
                    predicate: f => !f.Status.Equals(FrameworkStatus.DELETED));

                var frameworkDtos = new List<FrameworkListDto>();
                foreach (var framework in frameworks.Items)
                {
                    var dto = framework.ToListDto();
                    dto.VersionCount = await _unitOfWork.FrameworkVersions.CountAsync(v => v.FrameworkId == framework.Id);
                    frameworkDtos.Add(dto);
                }

                var pagedResult = new PagedList<FrameworkListDto>(
                    frameworkDtos,
                    frameworks.TotalCount,
                    frameworks.PageNumber,
                    frameworks.PageSize);

                return ApiResponse<PagedList<FrameworkListDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting frameworks");
                return ApiResponse<PagedList<FrameworkListDto>>.ErrorResponse("Failed to retrieve frameworks");
            }
        }

        public async Task<ApiResponse<FrameworkDto>> GetFrameworkByIdAsync(long id)
        {
            try
            {
                var framework = await _unitOfWork.Frameworks.GetFrameworkWithVersionsAsync(id);
                if (framework == null || framework.Status == FrameworkStatus.DELETED)
                {
                    return ApiResponse<FrameworkDto>.ErrorResponse("Framework not found", 404);
                }

                var frameworkDto = framework.ToDto();
                return ApiResponse<FrameworkDto>.SuccessResponse(frameworkDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting framework with ID {Id}", id);
                return ApiResponse<FrameworkDto>.ErrorResponse("Failed to retrieve framework");
            }
        }

        public async Task<ApiResponse<FrameworkDto>> CreateFrameworkAsync(CreateFrameworkDto createDto)
        {
            try
            {
                // Check if framework with same name exists
                var existingFramework = await _unitOfWork.Frameworks.FindSingleWithIncludesAsync(
                    f => f.Name == createDto.Name && f.Status != FrameworkStatus.DELETED);

                if (existingFramework != null)
                {
                    return ApiResponse<FrameworkDto>.ErrorResponse("A framework with this name already exists");
                }

                var framework = createDto.ToEntity();
                framework.Status = FrameworkStatus.NEW;
                framework.CreationDate = DateTime.UtcNow;
                framework.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Frameworks.Add(framework);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Framework", framework.Id, "ADD", $"Created framework: {framework.Name}");

                var frameworkDto = framework.ToDto();
                return ApiResponse<FrameworkDto>.SuccessResponse(frameworkDto, "Framework created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating framework");
                return ApiResponse<FrameworkDto>.ErrorResponse("Failed to create framework");
            }
        }

        public async Task<ApiResponse<FrameworkDto>> UpdateFrameworkAsync(long id, UpdateFrameworkDto updateDto)
        {
            try
            {
                var framework = await _unitOfWork.Frameworks.GetByIdAsync(id);
                if (framework == null || framework.Status == FrameworkStatus.DELETED)
                {
                    return ApiResponse<FrameworkDto>.ErrorResponse("Framework not found", 404);
                }

                // Check if the framework is published
                if (framework.Status == FrameworkStatus.ACTIVE)
                {
                    return ApiResponse<FrameworkDto>.ErrorResponse("Cannot update a published framework");
                }

                // Check if framework with same name exists (except current one)
                var existingFramework = await _unitOfWork.Frameworks.FindSingleWithIncludesAsync(
                    f => f.Name == updateDto.Name && f.Id != id && f.Status != FrameworkStatus.DELETED);

                if (existingFramework != null)
                {
                    return ApiResponse<FrameworkDto>.ErrorResponse("A framework with this name already exists");
                }

                // Update framework
                framework.Name = updateDto.Name;
                framework.Description = updateDto.Description;
                framework.Status = updateDto.Status;
                framework.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Frameworks.Update(framework);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Framework", framework.Id, "EDIT", $"Updated framework: {framework.Name}");

                // Get updated framework with versions
                var updatedFramework = await _unitOfWork.Frameworks.GetFrameworkWithVersionsAsync(id);
                var frameworkDto = updatedFramework.ToDto();

                return ApiResponse<FrameworkDto>.SuccessResponse(frameworkDto, "Framework updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating framework with ID {Id}", id);
                return ApiResponse<FrameworkDto>.ErrorResponse("Failed to update framework");
            }
        }

        public async Task<ApiResponse<bool>> DeleteFrameworkAsync(long id)
        {
            try
            {
                var framework = await _unitOfWork.Frameworks.GetByIdAsync(id);
                if (framework == null || framework.Status == FrameworkStatus.DELETED)
                {
                    return ApiResponse<bool>.ErrorResponse("Framework not found", 404);
                }

                // Check if the framework is published and being used by organizations
                if (framework.Status == FrameworkStatus.ACTIVE)
                {
                    var membershipCount = await _unitOfWork.Organizations.CountAsync(
                        o => o.Memberships.Any(m => m.FrameworkId == id &&
                                                  m.Status != OrganizationMembershipStatus.DELETED));

                    if (membershipCount > 0)
                    {
                        return ApiResponse<bool>.ErrorResponse("Cannot delete a framework that is in use by organizations");
                    }
                }

                // Soft delete the framework
                framework.Status = FrameworkStatus.DELETED;
                framework.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Frameworks.Update(framework);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Framework", framework.Id, "DELETE", $"Deleted framework: {framework.Name}");

                return ApiResponse<bool>.SuccessResponse(true, "Framework deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting framework with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete framework");
            }
        }

        public async Task<ApiResponse<FrameworkVersionDto>> AddVersionAsync(CreateFrameworkVersionDto createDto)
        {
            try
            {
                // Verify framework exists
                var framework = await _unitOfWork.Frameworks.GetByIdAsync(createDto.FrameworkId);
                if (framework == null || framework.Status == FrameworkStatus.DELETED)
                {
                    return ApiResponse<FrameworkVersionDto>.ErrorResponse("Framework not found", 404);
                }

                // Check if version with same name exists for this framework
                var existingVersion = await _unitOfWork.FrameworkVersions.FindSingleWithIncludesAsync(
                    v => v.FrameworkId == createDto.FrameworkId &&
                         v.Name == createDto.Name &&
                         v.Status != FrameworkVersionStatus.DELETED);

                if (existingVersion != null)
                {
                    return ApiResponse<FrameworkVersionDto>.ErrorResponse("A version with this name already exists for this framework");
                }

                var version = createDto.ToEntity();
                version.Status = FrameworkVersionStatus.NEW;
                version.CreationDate = DateTime.UtcNow;
                version.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.FrameworkVersions.Add(version);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("FrameworkVersion", version.Id, "ADD",
                    $"Added version {version.Name} to framework {framework.Name}");

                var versionDto = version.ToDto();
                return ApiResponse<FrameworkVersionDto>.SuccessResponse(versionDto, "Framework version created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding version to framework {FrameworkId}", createDto.FrameworkId);
                return ApiResponse<FrameworkVersionDto>.ErrorResponse("Failed to add framework version");
            }
        }

        public async Task<ApiResponse<FrameworkVersionDto>> UpdateVersionAsync(long id, UpdateFrameworkVersionDto updateDto)
        {
            try
            {
                var version = await _unitOfWork.FrameworkVersions.GetByIdAsync(id);
                if (version == null || version.Status == FrameworkVersionStatus.DELETED)
                {
                    return ApiResponse<FrameworkVersionDto>.ErrorResponse("Framework version not found", 404);
                }

                // Check if the version is published
                if (version.Status == FrameworkVersionStatus.PUBLISED)
                {
                    return ApiResponse<FrameworkVersionDto>.ErrorResponse("Cannot update a published version");
                }

                // Check if version with same name exists (except current one)
                var existingVersion = await _unitOfWork.FrameworkVersions.FindSingleWithIncludesAsync(
                    v => v.FrameworkId == version.FrameworkId &&
                         v.Name == updateDto.Name &&
                         v.Id != id &&
                         v.Status != FrameworkVersionStatus.DELETED);

                if (existingVersion != null)
                {
                    return ApiResponse<FrameworkVersionDto>.ErrorResponse("A version with this name already exists for this framework");
                }

                // Update version
                version.Name = updateDto.Name;
                version.Description = updateDto.Description;
                version.Status = updateDto.Status;
                version.VersionDate = updateDto.VersionDate;
                version.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.FrameworkVersions.Update(version);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("FrameworkVersion", version.Id, "EDIT",
                    $"Updated version: {version.Name}");

                var versionDto = version.ToDto();
                return ApiResponse<FrameworkVersionDto>.SuccessResponse(versionDto, "Framework version updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating framework version with ID {Id}", id);
                return ApiResponse<FrameworkVersionDto>.ErrorResponse("Failed to update framework version");
            }
        }

        public async Task<ApiResponse<bool>> DeleteVersionAsync(long id)
        {
            try
            {
                var version = await _unitOfWork.FrameworkVersions.GetByIdAsync(id);
                if (version == null || version.Status == FrameworkVersionStatus.DELETED)
                {
                    return ApiResponse<bool>.ErrorResponse("Framework version not found", 404);
                }

                // Check if the version is published and being used by organizations
                if (version.Status == FrameworkVersionStatus.PUBLISED)
                {
                    var membershipCount = await _unitOfWork.Organizations.CountAsync(
                        o => o.Memberships.Any(m => m.FrameworkVersionId == id &&
                                                  m.Status != OrganizationMembershipStatus.DELETED));

                    if (membershipCount > 0)
                    {
                        return ApiResponse<bool>.ErrorResponse("Cannot delete a version that is in use by organizations");
                    }
                }

                // Soft delete the version
                version.Status = FrameworkVersionStatus.DELETED;
                version.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.FrameworkVersions.Update(version);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("FrameworkVersion", version.Id, "DELETE",
                    $"Deleted version: {version.Name}");

                return ApiResponse<bool>.SuccessResponse(true, "Framework version deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting framework version with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete framework version");
            }
        }

        public async Task<ApiResponse<bool>> PublishFrameworkAsync(long id)
        {
            try
            {
                var framework = await _unitOfWork.Frameworks.GetFrameworkWithVersionsAsync(id);
                if (framework == null || framework.Status == FrameworkStatus.DELETED)
                {
                    return ApiResponse<bool>.ErrorResponse("Framework not found", 404);
                }

                // Check if the framework is already published
                if (framework.Status == FrameworkStatus.ACTIVE)
                {
                    return ApiResponse<bool>.ErrorResponse("Framework is already published");
                }

                // Check if the framework has at least one version
                if (framework.Versions == null || !framework.Versions.Any())
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot publish a framework without versions");
                }

                // Update framework status
                framework.Status = FrameworkStatus.ACTIVE;
                framework.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Frameworks.Update(framework);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Framework", framework.Id, "EDIT",
                    $"Published framework: {framework.Name}");

                return ApiResponse<bool>.SuccessResponse(true, "Framework published successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing framework with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to publish framework");
            }
        }
    }
}