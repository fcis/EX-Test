using Application.DTOs.Organization;
using Application.Interfaces;
using Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<OrganizationsController> _logger;

        public OrganizationsController(
            IOrganizationService organizationService,
            ILogger<OrganizationsController> logger)
        {
            _organizationService = organizationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all organizations with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOrganizations([FromQuery] PagingParameters pagingParameters)
        {
            var response = await _organizationService.GetOrganizationsAsync(pagingParameters);

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
        /// Get organization by id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOrganizationById(long id)
        {
            var response = await _organizationService.GetOrganizationByIdAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Create a new organization
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationDto createDto)
        {
            var response = await _organizationService.CreateOrganizationAsync(createDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return CreatedAtAction(nameof(GetOrganizationById), new { id = response.Data.Id }, response);
        }

        /// <summary>
        /// Update an existing organization
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateOrganization(long id, [FromBody] UpdateOrganizationDto updateDto)
        {
            var response = await _organizationService.UpdateOrganizationAsync(id, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Delete an organization
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteOrganization(long id)
        {
            var response = await _organizationService.DeleteOrganizationAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Add department to organization
        /// </summary>
        [HttpPost("{organizationId}/departments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddOrganizationDepartment(long organizationId, [FromBody] CreateOrganizationDepartmentDto createDto)
        {
            var response = await _organizationService.AddDepartmentAsync(organizationId, createDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return CreatedAtAction(nameof(GetOrganizationById), new { id = organizationId }, response);
        }

        /// <summary>
        /// Update organization department
        /// </summary>
        [HttpPut("departments/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateOrganizationDepartment(long id, [FromBody] UpdateOrganizationDepartmentDto updateDto)
        {
            var response = await _organizationService.UpdateDepartmentAsync(id, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Delete organization department
        /// </summary>
        [HttpDelete("departments/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteOrganizationDepartment(long id)
        {
            var response = await _organizationService.DeleteDepartmentAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Assign framework to organization
        /// </summary>
        [HttpPost("{organizationId}/frameworks/{frameworkId}/versions/{frameworkVersionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AssignFramework(long organizationId, long frameworkId, long frameworkVersionId)
        {
            var response = await _organizationService.AssignFrameworkAsync(organizationId, frameworkId, frameworkVersionId);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }
    }
}