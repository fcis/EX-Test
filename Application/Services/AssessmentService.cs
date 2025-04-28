using Application.DTOs.Assessment;
using Application.Interfaces;
using Application.Mappings;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AssessmentService> _logger;
        private readonly string _documentStoragePath;

        public AssessmentService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ICurrentUserService currentUserService,
            ILogger<AssessmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _currentUserService = currentUserService;
            _logger = logger;

            // Configure document storage path (in a real app, this would come from configuration)
            _documentStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documents");
            if (!Directory.Exists(_documentStoragePath))
            {
                Directory.CreateDirectory(_documentStoragePath);
            }
        }

        public async Task<ApiResponse<PagedList<AssessmentListDto>>> GetAssessmentsAsync(PagingParameters pagingParameters)
        {
            try
            {
                var assessments = await _unitOfWork.Assessments.GetPagedListAsync(
                    pagingParameters,
                    predicate: a => !a.Deleted,
                    includes: new List<System.Linq.Expressions.Expression<Func<Assessment, object>>>
                    {
                        a => a.Organization,
                        a => a.FrameworkVersion,
                        a => a.FrameworkVersion.Framework,
                        a => a.AssessmentItems
                    });

                var assessmentDtos = assessments.Items.Select(a => a.ToListDto()).ToList();

                var pagedResult = new PagedList<AssessmentListDto>(
                    assessmentDtos,
                    assessments.TotalCount,
                    assessments.PageNumber,
                    assessments.PageSize);

                return ApiResponse<PagedList<AssessmentListDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessments");
                return ApiResponse<PagedList<AssessmentListDto>>.ErrorResponse("Failed to retrieve assessments");
            }
        }

        public async Task<ApiResponse<PagedList<AssessmentListDto>>> GetAssessmentsByOrganizationAsync(long organizationId, PagingParameters pagingParameters)
        {
            try
            {
                var assessments = await _unitOfWork.Assessments.GetAssessmentsByOrganizationAsync(organizationId, pagingParameters);

                var assessmentDtos = assessments.Items.Select(a => a.ToListDto()).ToList();

                var pagedResult = new PagedList<AssessmentListDto>(
                    assessmentDtos,
                    assessments.TotalCount,
                    assessments.PageNumber,
                    assessments.PageSize);

                return ApiResponse<PagedList<AssessmentListDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessments for organization with ID {OrganizationId}", organizationId);
                return ApiResponse<PagedList<AssessmentListDto>>.ErrorResponse("Failed to retrieve assessments");
            }
        }

        public async Task<ApiResponse<AssessmentDto>> GetAssessmentByIdAsync(long id)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetAssessmentWithItemsAsync(id);
                if (assessment == null || assessment.Deleted)
                {
                    return ApiResponse<AssessmentDto>.ErrorResponse("Assessment not found", 404);
                }

                var assessmentDto = assessment.ToDto();
                return ApiResponse<AssessmentDto>.SuccessResponse(assessmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment with ID {Id}", id);
                return ApiResponse<AssessmentDto>.ErrorResponse("Failed to retrieve assessment");
            }
        }

        public async Task<ApiResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentDto createDto)
        {
            try
            {
                // Check if the organization and framework version exist
                var organization = await _unitOfWork.Organizations.GetByIdAsync(createDto.OrganizationId);
                if (organization == null || organization.Status == OrganizationStatus.DELETED)
                {
                    return ApiResponse<AssessmentDto>.ErrorResponse("Organization not found", 404);
                }

                var framework = await _unitOfWork.Frameworks.GetByIdAsync(createDto.FrameworkId);
                if (framework == null || framework.Status == FrameworkStatus.DELETED)
                {
                    return ApiResponse<AssessmentDto>.ErrorResponse("Framework not found", 404);
                }

                var frameworkVersion = await _unitOfWork.FrameworkVersions.GetByIdAsync(createDto.FrameworkVersionId);
                if (frameworkVersion == null || frameworkVersion.Status == FrameworkVersionStatus.DELETED || frameworkVersion.FrameworkId != createDto.FrameworkId)
                {
                    return ApiResponse<AssessmentDto>.ErrorResponse("Framework version not found or does not belong to the framework", 404);
                }

                // Check if there's already an active assessment for this organization and framework version
                var existingAssessment = await _unitOfWork.Assessments.GetAssessmentByOrganizationAndFrameworkVersionAsync(
                    createDto.OrganizationId, createDto.FrameworkVersionId);

                if (existingAssessment != null &&
                    (existingAssessment.Status == AssessmentStatus.DRAFT || existingAssessment.Status == AssessmentStatus.IN_PROGRESS))
                {
                    return ApiResponse<AssessmentDto>.ErrorResponse(
                        "There is already an active assessment for this organization and framework version");
                }

                // Start transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var currentUserId = _currentUserService.UserId ?? 0;
                    var now = DateTime.UtcNow;

                    // Create the assessment using the mapper
                    var assessment = createDto.ToEntity();
                    assessment.CreationDate = now;
                    assessment.CreatedUser = currentUserId;
                    assessment.LastModificationDate = now;
                    assessment.LastModificationUser = currentUserId;

                    _unitOfWork.Assessments.Add(assessment);
                    await _unitOfWork.CompleteAsync();

                    // Retrieve framework categories and clauses to create assessment items
                    var frameworkCategories = await _unitOfWork.FrameworkCategories.FindAsync(
                        fc => fc.FrameworkVersionId == createDto.FrameworkVersionId && !fc.Deleted);

                    foreach (var category in frameworkCategories)
                    {
                        var clauses = await _unitOfWork.FwCatClauses.FindAsync(
                            fcc => fcc.FrameworkCategoryId == category.Id && !fcc.Deleted);

                        foreach (var clauseLink in clauses)
                        {
                            var clause = await _unitOfWork.Clauses.GetByIdAsync(clauseLink.ClauseId);
                            if (clause != null && !clause.Deleted)
                            {
                                // Create assessment item for this clause
                                var assessmentItem = new AssessmentItem
                                {
                                    AssessmentId = assessment.Id,
                                    ClauseId = clause.Id,
                                    Status = ComplianceStatus.NOT_ASSESSED,
                                    Deleted = false,
                                    CreationDate = now,
                                    CreatedUser = currentUserId,
                                    LastModificationDate = now,
                                    LastModificationUser = currentUserId
                                };

                                _unitOfWork.AssessmentItems.Add(assessmentItem);
                                await _unitOfWork.CompleteAsync();

                                // Get checklist items for this clause and create assessment checklist items
                                var checklistItems = await _unitOfWork.ClauseCheckLists.GetCheckListsByClauseAsync(clause.Id);
                                foreach (var checklistItem in checklistItems)
                                {
                                    var assessmentChecklistItem = new AssessmentItemCheckList
                                    {
                                        AssessmentItemId = assessmentItem.Id,
                                        CheckListId = checklistItem.Id,
                                        IsChecked = false,
                                        Deleted = false,
                                        CreationDate = now,
                                        CreatedUser = currentUserId,
                                        LastModificationDate = now,
                                        LastModificationUser = currentUserId
                                    };

                                    _unitOfWork.AssessmentItemCheckLists.Add(assessmentChecklistItem);
                                }
                            }
                        }
                    }

                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    await _auditService.LogActionAsync("Assessment", assessment.Id, "ADD",
                        $"Created assessment for organization {organization.Name} and framework {framework.Name} version {frameworkVersion.Name}");

                    // Get the full assessment DTO to return
                    var assessmentDto = await GetAssessmentByIdAsync(assessment.Id);
                    return assessmentDto;
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assessment");
                return ApiResponse<AssessmentDto>.ErrorResponse("Failed to create assessment");
            }
        }

        public async Task<ApiResponse<AssessmentDto>> UpdateAssessmentAsync(long id, UpdateAssessmentDto updateDto)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null || assessment.Deleted)
                {
                    return ApiResponse<AssessmentDto>.ErrorResponse("Assessment not found", 404);
                }

                var currentUserId = _currentUserService.UserId ?? 0;

                // Update assessment
                assessment.Status = updateDto.Status;
                assessment.Notes = updateDto.Notes;
                assessment.CompletionDate = updateDto.CompletionDate;
                assessment.LastModificationDate = DateTime.UtcNow;
                assessment.LastModificationUser = currentUserId;

                _unitOfWork.Assessments.Update(assessment);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Assessment", assessment.Id, "EDIT",
                    $"Updated assessment status to {updateDto.Status}");

                // Get the updated assessment DTO to return
                var assessmentDto = await GetAssessmentByIdAsync(id);
                return assessmentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assessment with ID {Id}", id);
                return ApiResponse<AssessmentDto>.ErrorResponse("Failed to update assessment");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAssessmentAsync(long id)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessment == null || assessment.Deleted)
                {
                    return ApiResponse<bool>.ErrorResponse("Assessment not found", 404);
                }

                // Soft delete the assessment
                assessment.Deleted = true;
                assessment.LastModificationDate = DateTime.UtcNow;
                assessment.LastModificationUser = _currentUserService.UserId ?? 0;

                _unitOfWork.Assessments.Update(assessment);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Assessment", assessment.Id, "DELETE",
                    $"Deleted assessment");

                return ApiResponse<bool>.SuccessResponse(true, "Assessment deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assessment with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete assessment");
            }
        }

        public async Task<ApiResponse<PagedList<AssessmentItemDto>>> GetAssessmentItemsAsync(long assessmentId, PagingParameters pagingParameters)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
                if (assessment == null || assessment.Deleted)
                {
                    return ApiResponse<PagedList<AssessmentItemDto>>.ErrorResponse("Assessment not found", 404);
                }

                var assessmentItems = await _unitOfWork.AssessmentItems.GetAssessmentItemsByAssessmentAsync(
                    assessmentId, pagingParameters);

                var assessmentItemDtos = new List<AssessmentItemDto>();
                foreach (var item in assessmentItems.Items)
                {
                    // Load related data
                    await _unitOfWork.AssessmentItemDocuments.FindAsync(d => d.AssessmentItemId == item.Id && !d.Deleted);
                    await _unitOfWork.AssessmentItemCheckLists.FindAsync(cl => cl.AssessmentItemId == item.Id && !cl.Deleted);

                    // Use mapper
                    assessmentItemDtos.Add(item.ToDto());
                }

                var pagedResult = new PagedList<AssessmentItemDto>(
                    assessmentItemDtos,
                    assessmentItems.TotalCount,
                    assessmentItems.PageNumber,
                    assessmentItems.PageSize);

                return ApiResponse<PagedList<AssessmentItemDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment items for assessment ID {AssessmentId}", assessmentId);
                return ApiResponse<PagedList<AssessmentItemDto>>.ErrorResponse("Failed to retrieve assessment items");
            }
        }

        public async Task<ApiResponse<AssessmentItemDto>> GetAssessmentItemByIdAsync(long id)
        {
            try
            {
                var assessmentItem = await _unitOfWork.AssessmentItems.GetAssessmentItemWithDetailsAsync(id);
                if (assessmentItem == null || assessmentItem.Deleted)
                {
                    return ApiResponse<AssessmentItemDto>.ErrorResponse("Assessment item not found", 404);
                }

                // Use mapper
                var assessmentItemDto = assessmentItem.ToDto();
                return ApiResponse<AssessmentItemDto>.SuccessResponse(assessmentItemDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment item with ID {Id}", id);
                return ApiResponse<AssessmentItemDto>.ErrorResponse("Failed to retrieve assessment item");
            }
        }

        public async Task<ApiResponse<AssessmentItemDto>> UpdateAssessmentItemAsync(long id, UpdateAssessmentItemDto updateDto)
        {
            try
            {
                var assessmentItem = await _unitOfWork.AssessmentItems.GetByIdAsync(id);
                if (assessmentItem == null || assessmentItem.Deleted)
                {
                    return ApiResponse<AssessmentItemDto>.ErrorResponse("Assessment item not found", 404);
                }

                // If department is specified, verify it exists
                if (updateDto.AssignedDepartmentId.HasValue)
                {
                    var department = await _unitOfWork.OrganizationDepartments.GetByIdAsync(updateDto.AssignedDepartmentId.Value);
                    if (department == null || department.Deleted)
                    {
                        return ApiResponse<AssessmentItemDto>.ErrorResponse("Department not found", 404);
                    }
                }

                var currentUserId = _currentUserService.UserId ?? 0;

                // Update assessment item
                assessmentItem.Status = updateDto.Status;
                assessmentItem.Notes = updateDto.Notes;
                assessmentItem.CorrectiveActions = updateDto.CorrectiveActions;
                assessmentItem.AssignedDepartmentId = updateDto.AssignedDepartmentId;
                assessmentItem.DueDate = updateDto.DueDate;
                assessmentItem.LastModificationDate = DateTime.UtcNow;
                assessmentItem.LastModificationUser = currentUserId;

                _unitOfWork.AssessmentItems.Update(assessmentItem);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("AssessmentItem", assessmentItem.Id, "EDIT",
                    $"Updated assessment item status to {updateDto.Status}");

                // Get the updated assessment item DTO to return
                var assessmentItemDto = await GetAssessmentItemByIdAsync(id);
                return assessmentItemDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assessment item with ID {Id}", id);
                return ApiResponse<AssessmentItemDto>.ErrorResponse("Failed to update assessment item");
            }
        }

        public async Task<ApiResponse<AssessmentItemDocumentDto>> UploadDocumentAsync(UploadAssessmentDocumentDto uploadDto)
        {
            try
            {
                var assessmentItem = await _unitOfWork.AssessmentItems.GetByIdAsync(uploadDto.AssessmentItemId);
                if (assessmentItem == null || assessmentItem.Deleted)
                {
                    return ApiResponse<AssessmentItemDocumentDto>.ErrorResponse("Assessment item not found", 404);
                }

                // If department is specified, verify it exists
                if (uploadDto.DepartmentId.HasValue)
                {
                    var department = await _unitOfWork.OrganizationDepartments.GetByIdAsync(uploadDto.DepartmentId.Value);
                    if (department == null || department.Deleted)
                    {
                        return ApiResponse<AssessmentItemDocumentDto>.ErrorResponse("Department not found", 404);
                    }
                }

                var currentUserId = _currentUserService.UserId ?? 0;
                var now = DateTime.UtcNow;

                // Generate a unique filename to avoid collisions
                var originalFileName = Path.GetFileName(uploadDto.File.FileName);
                var fileExtension = Path.GetExtension(originalFileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_documentStoragePath, uniqueFileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadDto.File.CopyToAsync(stream);
                }

                // Create document record
                var document = new AssessmentItemDocument
                {
                    AssessmentItemId = uploadDto.AssessmentItemId,
                    FileName = originalFileName,
                    StoragePath = uniqueFileName,
                    ContentType = uploadDto.File.ContentType,
                    FileSize = uploadDto.File.Length,
                    UploadDate = now,
                    DocumentType = uploadDto.DocumentType,
                    ReleaseDate = uploadDto.ReleaseDate,
                    DepartmentId = uploadDto.DepartmentId,
                    Deleted = false,
                    CreationDate = now,
                    CreatedUser = currentUserId,
                    LastModificationDate = now,
                    LastModificationUser = currentUserId
                };

                _unitOfWork.AssessmentItemDocuments.Add(document);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("AssessmentItemDocument", document.Id, "ADD",
                    $"Uploaded document: {originalFileName}");

                // Use mapper
                var documentDto = document.ToDto();
                return ApiResponse<AssessmentItemDocumentDto>.SuccessResponse(documentDto, "Document uploaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for assessment item ID {AssessmentItemId}", uploadDto.AssessmentItemId);
                return ApiResponse<AssessmentItemDocumentDto>.ErrorResponse("Failed to upload document");
            }
        }

        public async Task<ApiResponse<bool>> DeleteDocumentAsync(long id)
        {
            try
            {
                var document = await _unitOfWork.AssessmentItemDocuments.GetByIdAsync(id);
                if (document == null || document.Deleted)
                {
                    return ApiResponse<bool>.ErrorResponse("Document not found", 404);
                }

                // Soft delete the document
                document.Deleted = true;
                document.LastModificationDate = DateTime.UtcNow;
                document.LastModificationUser = _currentUserService.UserId ?? 0;

                _unitOfWork.AssessmentItemDocuments.Update(document);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("AssessmentItemDocument", document.Id, "DELETE",
                    $"Deleted document: {document.FileName}");

                return ApiResponse<bool>.SuccessResponse(true, "Document deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete document");
            }
        }

        public async Task<ApiResponse<byte[]>> DownloadDocumentAsync(long id)
        {
            try
            {
                var document = await _unitOfWork.AssessmentItemDocuments.GetByIdAsync(id);
                if (document == null || document.Deleted)
                {
                    return ApiResponse<byte[]>.ErrorResponse("Document not found", 404);
                }

                var filePath = Path.Combine(_documentStoragePath, document.StoragePath);
                if (!File.Exists(filePath))
                {
                    return ApiResponse<byte[]>.ErrorResponse("Document file not found", 404);
                }

                var fileBytes = await File.ReadAllBytesAsync(filePath);

                await _auditService.LogActionAsync("AssessmentItemDocument", document.Id, "DOWNLOAD",
                    $"Downloaded document: {document.FileName}");

                return ApiResponse<byte[]>.SuccessResponse(fileBytes, "Document downloaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document with ID {Id}", id);
                return ApiResponse<byte[]>.ErrorResponse("Failed to download document");
            }
        }

        public async Task<ApiResponse<AssessmentItemCheckListDto>> UpdateCheckListItemAsync(long assessmentItemId, UpdateAssessmentItemCheckListDto updateDto)
        {
            try
            {
                var assessmentItem = await _unitOfWork.AssessmentItems.GetByIdAsync(assessmentItemId);
                if (assessmentItem == null || assessmentItem.Deleted)
                {
                    return ApiResponse<AssessmentItemCheckListDto>.ErrorResponse("Assessment item not found", 404);
                }

                // Check if the checklist item exists
                var checkList = await _unitOfWork.ClauseCheckLists.GetByIdAsync(updateDto.CheckListId);
                if (checkList == null || checkList.Deleted)
                {
                    return ApiResponse<AssessmentItemCheckListDto>.ErrorResponse("Checklist item not found", 404);
                }

                // Check if the assessment checklist item already exists
                var assessmentCheckList = await _unitOfWork.AssessmentItemCheckLists.GetByAssessmentItemAndCheckListAsync(
                    assessmentItemId, updateDto.CheckListId);

                var currentUserId = _currentUserService.UserId ?? 0;
                var now = DateTime.UtcNow;

                if (assessmentCheckList == null)
                {
                    // Create a new assessment checklist item
                    assessmentCheckList = new AssessmentItemCheckList
                    {
                        AssessmentItemId = assessmentItemId,
                        CheckListId = updateDto.CheckListId,
                        IsChecked = updateDto.IsChecked,
                        Notes = updateDto.Notes,
                        Deleted = false,
                        CreationDate = now,
                        CreatedUser = currentUserId,
                        LastModificationDate = now,
                        LastModificationUser = currentUserId
                    };

                    _unitOfWork.AssessmentItemCheckLists.Add(assessmentCheckList);
                    await _unitOfWork.CompleteAsync();

                    await _auditService.LogActionAsync("AssessmentItemCheckList", assessmentCheckList.Id, "ADD",
                        $"Added checklist item: {checkList.Name}, IsChecked: {updateDto.IsChecked}");
                }
                else
                {
                    // Update the existing assessment checklist item
                    assessmentCheckList.IsChecked = updateDto.IsChecked;
                    assessmentCheckList.Notes = updateDto.Notes;
                    assessmentCheckList.LastModificationDate = now;
                    assessmentCheckList.LastModificationUser = currentUserId;

                    _unitOfWork.AssessmentItemCheckLists.Update(assessmentCheckList);
                    await _unitOfWork.CompleteAsync();

                    await _auditService.LogActionAsync("AssessmentItemCheckList", assessmentCheckList.Id, "EDIT",
                        $"Updated checklist item: {checkList.Name}, IsChecked: {updateDto.IsChecked}");
                }

                // Use mapper
                assessmentCheckList.CheckList = checkList; // Ensure the navigation property is set for mapping
                var checkListDto = assessmentCheckList.ToDto();
                return ApiResponse<AssessmentItemCheckListDto>.SuccessResponse(checkListDto, "Checklist item updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating checklist item for assessment item ID {AssessmentItemId}", assessmentItemId);
                return ApiResponse<AssessmentItemCheckListDto>.ErrorResponse("Failed to update checklist item");
            }
        }

        public async Task<ApiResponse<GapAnalysisDto>> GenerateGapAnalysisAsync(long assessmentId)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetAssessmentWithItemsAsync(assessmentId);
                if (assessment == null || assessment.Deleted)
                {
                    return ApiResponse<GapAnalysisDto>.ErrorResponse("Assessment not found", 404);
                }

                // Calculate overall statistics
                var totalControls = assessment.AssessmentItems.Count;
                var conformingControls = assessment.AssessmentItems.Count(ai => ai.Status == ComplianceStatus.CONFORMITY);
                var nonConformingControls = assessment.AssessmentItems.Count(ai => ai.Status == ComplianceStatus.NON_CONFORMITY);
                var partiallyConformingControls = assessment.AssessmentItems.Count(ai => ai.Status == ComplianceStatus.CONFORMITY_WITH_NOTES);
                var notAssessedControls = assessment.AssessmentItems.Count(ai => ai.Status == ComplianceStatus.NOT_ASSESSED);

                decimal conformityPercentage = totalControls > 0
                    ? (decimal)conformingControls / totalControls * 100
                    : 0;

                // Calculate statistics by department
                var departmentGaps = new List<DepartmentGapDto>();
                var departments = assessment.AssessmentItems
                    .Where(ai => ai.AssignedDepartmentId.HasValue)
                    .Select(ai => ai.AssignedDepartmentId.Value)
                    .Distinct()
                    .ToList();

                foreach (var departmentId in departments)
                {
                    var departmentItems = assessment.AssessmentItems
                        .Where(ai => ai.AssignedDepartmentId == departmentId)
                        .ToList();

                    var departmentTotalControls = departmentItems.Count;
                    var departmentConforming = departmentItems.Count(ai => ai.Status == ComplianceStatus.CONFORMITY);
                    var departmentNonConforming = departmentItems.Count(ai => ai.Status == ComplianceStatus.NON_CONFORMITY);
                    var departmentPartiallyConforming = departmentItems.Count(ai => ai.Status == ComplianceStatus.CONFORMITY_WITH_NOTES);
                    var departmentNotAssessed = departmentItems.Count(ai => ai.Status == ComplianceStatus.NOT_ASSESSED);

                    decimal departmentConformityPercentage = departmentTotalControls > 0
                        ? (decimal)departmentConforming / departmentTotalControls * 100
                        : 0;

                    var department = await _unitOfWork.OrganizationDepartments.GetByIdAsync(departmentId);

                    departmentGaps.Add(new DepartmentGapDto
                    {
                        DepartmentId = departmentId,
                        DepartmentName = department?.Name ?? "Unknown Department",
                        TotalControls = departmentTotalControls,
                        ConformingControls = departmentConforming,
                        NonConformingControls = departmentNonConforming,
                        PartiallyConformingControls = departmentPartiallyConforming,
                        NotAssessedControls = departmentNotAssessed,
                        ConformityPercentage = departmentConformityPercentage
                    });
                }

                // Calculate statistics by category (requires additional data retrieval)
                var categoryGaps = new List<CategoryGapDto>();

                // For simplicity, we'll leave this as a placeholder. In a real implementation,
                // you would need to retrieve the relationship between clauses and categories.

                var gapAnalysis = new GapAnalysisDto
                {
                    AssessmentId = assessment.Id,
                    OrganizationName = assessment.Organization.Name,
                    FrameworkName = assessment.FrameworkVersion.Framework.Name,
                    FrameworkVersionName = assessment.FrameworkVersion.Name,
                    GeneratedDate = DateTime.UtcNow,
                    TotalControls = totalControls,
                    ConformingControls = conformingControls,
                    NonConformingControls = nonConformingControls,
                    PartiallyConformingControls = partiallyConformingControls,
                    NotAssessedControls = notAssessedControls,
                    ConformityPercentage = conformityPercentage,
                    DepartmentGaps = departmentGaps,
                    CategoryGaps = categoryGaps
                };

                await _auditService.LogActionAsync("Assessment", assessment.Id, "REPORT",
                    $"Generated gap analysis report");

                return ApiResponse<GapAnalysisDto>.SuccessResponse(gapAnalysis, "Gap analysis generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating gap analysis for assessment ID {AssessmentId}", assessmentId);
                return ApiResponse<GapAnalysisDto>.ErrorResponse("Failed to generate gap analysis");
            }
        }

        public async Task<ApiResponse<GapAnalysisDto>> GenerateGapAnalysisByDepartmentAsync(long assessmentId, long departmentId)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetAssessmentWithItemsAsync(assessmentId);
                if (assessment == null || assessment.Deleted)
                {
                    return ApiResponse<GapAnalysisDto>.ErrorResponse("Assessment not found", 404);
                }

                var department = await _unitOfWork.OrganizationDepartments.GetByIdAsync(departmentId);
                if (department == null || department.Deleted)
                {
                    return ApiResponse<GapAnalysisDto>.ErrorResponse("Department not found", 404);
                }

                // Filter assessment items by department
                var departmentItems = assessment.AssessmentItems
                    .Where(ai => ai.AssignedDepartmentId == departmentId)
                    .ToList();

                if (!departmentItems.Any())
                {
                    return ApiResponse<GapAnalysisDto>.ErrorResponse("No assessment items found for this department", 404);
                }

                // Calculate statistics
                var totalControls = departmentItems.Count;
                var conformingControls = departmentItems.Count(ai => ai.Status == ComplianceStatus.CONFORMITY);
                var nonConformingControls = departmentItems.Count(ai => ai.Status == ComplianceStatus.NON_CONFORMITY);
                var partiallyConformingControls = departmentItems.Count(ai => ai.Status == ComplianceStatus.CONFORMITY_WITH_NOTES);
                var notAssessedControls = departmentItems.Count(ai => ai.Status == ComplianceStatus.NOT_ASSESSED);

                decimal conformityPercentage = totalControls > 0
                    ? (decimal)conformingControls / totalControls * 100
                    : 0;

                var gapAnalysis = new GapAnalysisDto
                {
                    AssessmentId = assessment.Id,
                    OrganizationName = assessment.Organization.Name,
                    FrameworkName = assessment.FrameworkVersion.Framework.Name,
                    FrameworkVersionName = assessment.FrameworkVersion.Name,
                    GeneratedDate = DateTime.UtcNow,
                    TotalControls = totalControls,
                    ConformingControls = conformingControls,
                    NonConformingControls = nonConformingControls,
                    PartiallyConformingControls = partiallyConformingControls,
                    NotAssessedControls = notAssessedControls,
                    ConformityPercentage = conformityPercentage,
                    DepartmentGaps = new List<DepartmentGapDto>
                    {
                        new DepartmentGapDto
                        {
                            DepartmentId = departmentId,
                            DepartmentName = department.Name,
                            TotalControls = totalControls,
                            ConformingControls = conformingControls,
                            NonConformingControls = nonConformingControls,
                            PartiallyConformingControls = partiallyConformingControls,
                            NotAssessedControls = notAssessedControls,
                            ConformityPercentage = conformityPercentage
                        }
                    },
                    CategoryGaps = new List<CategoryGapDto>() // Empty for department-specific reports
                };

                await _auditService.LogActionAsync("Assessment", assessment.Id, "REPORT",
                    $"Generated gap analysis report for department: {department.Name}");

                return ApiResponse<GapAnalysisDto>.SuccessResponse(gapAnalysis, "Gap analysis generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating gap analysis for assessment ID {AssessmentId} and department ID {DepartmentId}",
                    assessmentId, departmentId);
                return ApiResponse<GapAnalysisDto>.ErrorResponse("Failed to generate gap analysis");
            }
        }

        public async Task<ApiResponse<GapAnalysisDto>> GenerateGapAnalysisByCategoryAsync(long assessmentId, long categoryId)
        {
            // Similar to the department method, but filtering by category
            // This would require additional queries to relate clauses to categories
            // For brevity, we'll leave this as a placeholder

            return ApiResponse<GapAnalysisDto>.ErrorResponse("Not implemented yet");
        }

        public async Task<ApiResponse<Dictionary<string, int>>> GetAssessmentStatusSummaryAsync(long organizationId)
        {
            try
            {
                var assessments = await _unitOfWork.Assessments.FindAsync(
                    a => a.OrganizationId == organizationId && !a.Deleted);

                var statusSummary = new Dictionary<string, int>();
                foreach (var status in Enum.GetValues(typeof(AssessmentStatus)).Cast<AssessmentStatus>())
                {
                    statusSummary[status.ToString()] = assessments.Count(a => a.Status == status);
                }

                return ApiResponse<Dictionary<string, int>>.SuccessResponse(statusSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment status summary for organization ID {OrganizationId}", organizationId);
                return ApiResponse<Dictionary<string, int>>.ErrorResponse("Failed to retrieve assessment status summary");
            }
        }

        public async Task<ApiResponse<Dictionary<string, int>>> GetComplianceStatusSummaryAsync(long assessmentId)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
                if (assessment == null || assessment.Deleted)
                {
                    return ApiResponse<Dictionary<string, int>>.ErrorResponse("Assessment not found", 404);
                }

                var statusSummary = await _unitOfWork.AssessmentItems.GetStatusSummaryAsync(assessmentId);

                // Convert to string keys for the API response
                var result = statusSummary.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);

                return ApiResponse<Dictionary<string, int>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compliance status summary for assessment ID {AssessmentId}", assessmentId);
                return ApiResponse<Dictionary<string, int>>.ErrorResponse("Failed to retrieve compliance status summary");
            }
        }
    }
}