using Application.DTOs.Framework;
using Application.Interfaces;
using Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrameworksController : ControllerBase
    {
        private readonly IFrameworkService _frameworkService;
        private readonly ILogger<FrameworksController> _logger;

        public FrameworksController(
            IFrameworkService frameworkService,
            ILogger<FrameworksController> logger)
        {
            _frameworkService = frameworkService;
            _logger = logger;
        }

        /// <summary>
        /// Get all frameworks with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFrameworks([FromQuery] PagingParameters pagingParameters)
        {
            var response = await _frameworkService.GetFrameworksAsync(pagingParameters);

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
        /// Get framework by id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFrameworkById(long id)
        {
            var response = await _frameworkService.GetFrameworkByIdAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Create a new framework
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateFramework([FromBody] CreateFrameworkDto createDto)
        {
            var response = await _frameworkService.CreateFrameworkAsync(createDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return CreatedAtAction(nameof(GetFrameworkById), new { id = response.Data.Id }, response);
        }

        /// <summary>
        /// Update an existing framework
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateFramework(long id, [FromBody] UpdateFrameworkDto updateDto)
        {
            var response = await _frameworkService.UpdateFrameworkAsync(id, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Delete a framework
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteFramework(long id)
        {
            var response = await _frameworkService.DeleteFrameworkAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Publish a framework
        /// </summary>
        [HttpPost("{id}/publish")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PublishFramework(long id)
        {
            var response = await _frameworkService.PublishFrameworkAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Add version to framework
        /// </summary>
        [HttpPost("versions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddFrameworkVersion([FromBody] CreateFrameworkVersionDto createDto)
        {
            var response = await _frameworkService.AddVersionAsync(createDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return CreatedAtAction(nameof(GetFrameworkById), new { id = createDto.FrameworkId }, response);
        }

        /// <summary>
        /// Update framework version
        /// </summary>
        [HttpPut("versions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateFrameworkVersion(long id, [FromBody] UpdateFrameworkVersionDto updateDto)
        {
            var response = await _frameworkService.UpdateVersionAsync(id, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Delete framework version
        /// </summary>
        [HttpDelete("versions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteFrameworkVersion(long id)
        {
            var response = await _frameworkService.DeleteVersionAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }
    }
}