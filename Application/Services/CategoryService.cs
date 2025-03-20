using Application.DTOs.Category;
using Application.Interfaces;
using Application.Mappings;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{


    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService; 
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ICurrentUserService currentUserService, 
            ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _currentUserService = currentUserService; 
            _logger = logger;
        }

        public async Task<ApiResponse<PagedList<CategoryDto>>> GetCategoriesAsync(PagingParameters pagingParameters)
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetPagedListAsync(
                    pagingParameters,
                    predicate: c => !c.Deleted);

                var categoryDtos = new List<CategoryDto>();
                foreach (var category in categories.Items)
                {
                    var dto = category.ToDto();
                    dto.FrameworksCount = category.Frameworks.Count;
                    categoryDtos.Add(dto);
                }

                var pagedResult = new PagedList<CategoryDto>(
                    categoryDtos,
                    categories.TotalCount,
                    categories.PageNumber,
                    categories.PageSize);

                return ApiResponse<PagedList<CategoryDto>>.SuccessResponse(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return ApiResponse<PagedList<CategoryDto>>.ErrorResponse("Failed to retrieve categories");
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(long id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null || category.Deleted)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found", 404);
                }

                var categoryDto = category.ToDto();
                categoryDto.FrameworksCount = category.Frameworks.Count;

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category with ID {Id}", id);
                return ApiResponse<CategoryDto>.ErrorResponse("Failed to retrieve category");
            }
        }

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createDto)
        {
            try
            {
                // Check if category with same name exists
                var existingCategory = await _unitOfWork.Categories.FindSingleWithIncludesAsync(
                    c => c.Name == createDto.Name && !c.Deleted);

                if (existingCategory != null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("A category with this name already exists");
                }

                var category = createDto.ToEntity();
                category.Deleted = false;

                // Set creation/modification dates and user IDs
                var now = DateTime.UtcNow;
                var currentUserId = _currentUserService.UserId ?? 0; // Get current user ID

                category.CreationDate = now;
                category.LastModificationDate = now;
                category.CreatedUser = currentUserId;
                category.LastModificationUser = currentUserId;

                _unitOfWork.Categories.Add(category);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Category", category.Id, "ADD",
                    $"Created category: {category.Name}");

                var categoryDto = category.ToDto();
                categoryDto.FrameworksCount = 0;

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return ApiResponse<CategoryDto>.ErrorResponse("Failed to create category");
            }
        }
        public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(long id, UpdateCategoryDto updateDto)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null || category.Deleted)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found", 404);
                }

                // Check if category with same name exists (except current one)
                var existingCategory = await _unitOfWork.Categories.FindSingleWithIncludesAsync(
                    c => c.Name == updateDto.Name && c.Id != id && !c.Deleted);

                if (existingCategory != null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("A category with this name already exists");
                }

                // Update category
                category.Name = updateDto.Name;
                category.Description = updateDto.Description;
                category.LastModificationDate = DateTime.UtcNow;

                // Set last modification user
                var currentUserId = _currentUserService.UserId ?? 0;
                category.LastModificationUser = currentUserId;

                _unitOfWork.Categories.Update(category);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Category", category.Id, "EDIT",
                    $"Updated category: {category.Name}");

                var categoryDto = category.ToDto();
                categoryDto.FrameworksCount = category.Frameworks.Count;

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID {Id}", id);
                return ApiResponse<CategoryDto>.ErrorResponse("Failed to update category");
            }
        }
        public async Task<ApiResponse<bool>> DeleteCategoryAsync(long id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null || category.Deleted)
                {
                    return ApiResponse<bool>.ErrorResponse("Category not found", 404);
                }

                // Check if the category is used in any framework
                if (category.Frameworks.Any())
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete a category that is used in frameworks");
                }

                // Soft delete the category
                category.Deleted = true;
                category.LastModificationDate = DateTime.UtcNow;

                _unitOfWork.Categories.Update(category);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync("Category", category.Id, "DELETE",
                    $"Deleted category: {category.Name}");

                return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID {Id}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete category");
            }
        }
    }
}