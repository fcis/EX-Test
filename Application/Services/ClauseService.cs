using Application.DTOs.Clause;
using Application.Interfaces;
using Application.Mappings;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{


    public class ClauseService : IClauseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ILogger<ClauseService> _logger;

        public ClauseService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ILogger<ClauseService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedList<ClauseDto>>> GetClausesAsync(PagingParameters pagingParameters)
        {
            try
            {
                var clauses = await _unitOfWork.Clauses.GetPagedListAsync(
                    pagingParameters,
                    predicate: c => !c.Deleted);

                var clauseDtos = clauses.Items.ToDtoList(c => c.ToDto());

                var pagedResult = new PagedList<ClauseDto>(
                    clauseDtos,
                    clauses.TotalCount,
                    clauses.PageNumber,
                    clauses.PageSize);

                return ApiResponse<PagedList<ClauseDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clauses");
                return ApiResponse<PagedList<ClauseDto>>.ErrorResponse("Failed to retrieve clauses");
            }
        }

        public async Task<ApiResponse<ClauseDto>> GetClauseByIdAsync(long id)
        {
            try
            {
                var clause = await _unitOfWork.Clauses.FindSingleWithIncludesAsync(
                    c => c.Id == id && !c.Deleted,
                    includes: new List<System.Linq.Expressions.Expression<Func<Clause, object>>>
                    {
                        c => c.CheckLists
                    });

                if (clause == null)
                {
                    return ApiResponse<ClauseDto>.ErrorResponse("Clause not found", 404);
                }

                var clauseDto = clause.ToDto();
                return ApiResponse<ClauseDto>.SuccessResponse(clauseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clause with ID {Id}", id);
                return ApiResponse<ClauseDto>.ErrorResponse("Failed to retrieve clause");
            }
        }

        public async Task<ApiResponse<ClauseDto>> CreateClauseAsync(CreateClauseDto createDto)
        {
            try
            {
                // Check if clause with same name exists
                var existingClause = await _unitOfWork.Clauses.FindSingleWithIncludesAsync(
                    c => c.Name == createDto.Name && !c.Deleted);

                if (existingClause != null)
                {
                    return ApiResponse<ClauseDto>.ErrorResponse("A clause with this name already exists");
                }

                var clause = createDto.ToEntity();
                clause.Deleted = false;
                clause.CreationDate = DateTime.UtcNow;
                clause.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Clauses.Add(clause);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Clause", clause.Id, "ADD",
                    $"Created clause: {clause.Name}");

                var clauseDto = clause.ToDto();
                return ApiResponse<ClauseDto>.SuccessResponse(clauseDto, "Clause created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating clause");
                return ApiResponse<ClauseDto>.ErrorResponse("Failed to create clause");
            }
        }

        public async Task<ApiResponse<ClauseDto>> UpdateClauseAsync(long id, UpdateClauseDto updateDto)
        {
            try
            {
                var clause = await _unitOfWork.Clauses.FindSingleWithIncludesAsync(
                    c => c.Id == id && !c.Deleted,
                    includes: new List<System.Linq.Expressions.Expression<Func<Clause, object>>>
                    {
                        c => c.CheckLists
                    });

                if (clause == null)
                {
                    return ApiResponse<ClauseDto>.ErrorResponse("Clause not found", 404);
                }

                // Check if clause with same name exists (except current one)
                var existingClause = await _unitOfWork.Clauses.FindSingleWithIncludesAsync(
                    c => c.Name == updateDto.Name && c.Id != id && !c.Deleted);

                if (existingClause != null)
                {
                    return ApiResponse<ClauseDto>.ErrorResponse("A clause with this name already exists");
                }

                // Update clause
                clause.Name = updateDto.Name;
                clause.Description = updateDto.Description;
                clause.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Clauses.Update(clause);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Clause", clause.Id, "EDIT",
                    $"Updated clause: {clause.Name}");

                var clauseDto = clause.ToDto();
                return ApiResponse<ClauseDto>.SuccessResponse(clauseDto, "Clause updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating clause with ID {Id}", id);
                return ApiResponse<ClauseDto>.ErrorResponse("Failed to update clause");
            }
        }

        public async Task<ApiResponse<bool>> DeleteClauseAsync(long id)
        {
            try
            {
                var clause = await _unitOfWork.Clauses.GetByIdAsync(id);
                if (clause == null || clause.Deleted)
                {
                    return ApiResponse<bool>.ErrorResponse("Clause not found", 404);
                }

                // Check if the clause is used in any framework
                if (clause.FrameworkCategories.Any())
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete a clause that is used in frameworks");
                }

                // Soft delete the clause
                clause.Deleted = true;
                clause.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Clauses.Update(clause);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Clause", clause.Id, "DELETE",
                    $"Deleted clause: {clause.Name}");

                return ApiResponse<bool>.SuccessResponse(true, "Clause deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting clause with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete clause");
            }
        }

        public async Task<ApiResponse<ClauseCheckListDto>> AddCheckListItemAsync(CreateClauseCheckListDto createDto)
        {
            try
            {
                // Verify clause exists
                var clause = await _unitOfWork.Clauses.GetByIdAsync(createDto.ClauseId);
                if (clause == null || clause.Deleted)
                {
                    return ApiResponse<ClauseCheckListDto>.ErrorResponse("Clause not found", 404);
                }

                // Check if checklist item with same name exists for this clause
                var existingItem = await _unitOfWork.ClauseCheckLists.FindSingleWithIncludesAsync(
                    c => c.ClauseId == createDto.ClauseId && c.Name == createDto.Name && !c.Deleted);

                if (existingItem != null)
                {
                    return ApiResponse<ClauseCheckListDto>.ErrorResponse("A checklist item with this name already exists for this clause");
                }

                var checkListItem = createDto.ToEntity();
                checkListItem.Deleted = false;
                checkListItem.CreationDate = DateTime.UtcNow;
                checkListItem.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.ClauseCheckLists.Add(checkListItem);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("ClauseCheckList", checkListItem.Id, "ADD",
                    $"Added checklist item {checkListItem.Name} to clause {clause.Name}");

                var checkListItemDto = checkListItem.ToDto();
                return ApiResponse<ClauseCheckListDto>.SuccessResponse(checkListItemDto, "Checklist item added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding checklist item to clause {ClauseId}", createDto.ClauseId);
                return ApiResponse<ClauseCheckListDto>.ErrorResponse("Failed to add checklist item");
            }
        }

        public async Task<ApiResponse<ClauseCheckListDto>> UpdateCheckListItemAsync(long id, UpdateClauseCheckListDto updateDto)
        {
            try
            {
                var checkListItem = await _unitOfWork.ClauseCheckLists.GetByIdAsync(id);
                if (checkListItem == null || checkListItem.Deleted)
                {
                    return ApiResponse<ClauseCheckListDto>.ErrorResponse("Checklist item not found", 404);
                }

                // Check if checklist item with same name exists (except current one)
                var existingItem = await _unitOfWork.ClauseCheckLists.FindSingleWithIncludesAsync(
                    c => c.ClauseId == checkListItem.ClauseId &&
                         c.Name == updateDto.Name &&
                         c.Id != id &&
                         !c.Deleted);

                if (existingItem != null)
                {
                    return ApiResponse<ClauseCheckListDto>.ErrorResponse("A checklist item with this name already exists for this clause");
                }

                // Update checklist item
                checkListItem.Name = updateDto.Name;
                checkListItem.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.ClauseCheckLists.Update(checkListItem);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("ClauseCheckList", checkListItem.Id, "EDIT",
                    $"Updated checklist item: {checkListItem.Name}");

                var checkListItemDto = checkListItem.ToDto();
                return ApiResponse<ClauseCheckListDto>.SuccessResponse(checkListItemDto, "Checklist item updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating checklist item with ID {Id}", id);
                return ApiResponse<ClauseCheckListDto>.ErrorResponse("Failed to update checklist item");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCheckListItemAsync(long id)
        {
            try
            {
                var checkListItem = await _unitOfWork.ClauseCheckLists.GetByIdAsync(id);
                if (checkListItem == null || checkListItem.Deleted)
                {
                    return ApiResponse<bool>.ErrorResponse("Checklist item not found", 404);
                }

                // Check if the checklist item is referenced by organizations
                var orgAnswersCount = await _unitOfWork.OrganizationCheckListAnswers.CountAsync(
                    a => a.CheckId == id && !a.Deleted);

                if (orgAnswersCount > 0)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete a checklist item that is referenced by organizations");
                }

                // Soft delete the checklist item
                checkListItem.Deleted = true;
                checkListItem.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.ClauseCheckLists.Update(checkListItem);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("ClauseCheckList", checkListItem.Id, "DELETE",
                    $"Deleted checklist item: {checkListItem.Name}");

                return ApiResponse<bool>.SuccessResponse(true, "Checklist item deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting checklist item with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete checklist item");
            }
        }
    }
}