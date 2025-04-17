using Application.DTOs.Category;
using Application.Interfaces;
using Core.Common;
using Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories with pagination
        /// </summary>
        [HttpGet]
        [HasPermission(Constants.Permissions.ViewCategories)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategories([FromQuery] PagingParameters pagingParameters)
        {
            var response = await _categoryService.GetCategoriesAsync(pagingParameters);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            // Add pagination header
            Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
            {
                totalCount = response.Data.TotalCount,
                pageSize = response.Data.PageSize,
                currentPage = response.Data.PageNumber,
                totalPages = response.Data.TotalPages,
                hasPrevious = response.Data.HasPreviousPage,
                hasNext = response.Data.HasNextPage
            }));

            return Ok(response);
        }

        /// <summary>
        /// Get category by id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategoryById(long id)
        {
            var response = await _categoryService.GetCategoryByIdAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        [HasPermission(Constants.Permissions.CreateCategory)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            var response = await _categoryService.CreateCategoryAsync(createDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCategory(long id, [FromBody] UpdateCategoryDto updateDto)
        {
            var response = await _categoryService.UpdateCategoryAsync(id, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            var response = await _categoryService.DeleteCategoryAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }
    }
}