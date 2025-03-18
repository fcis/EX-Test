using Application.DTOs.Category;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<PagedList<CategoryDto>>> GetCategoriesAsync(PagingParameters pagingParameters);
        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(long id);
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createDto);
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(long id, UpdateCategoryDto updateDto);
        Task<ApiResponse<bool>> DeleteCategoryAsync(long id);
    }
}
