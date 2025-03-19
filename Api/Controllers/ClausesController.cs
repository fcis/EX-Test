using Application.DTOs.Clause;
using Application.Interfaces;
using Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClausesController : ControllerBase
    {
        private readonly IClauseService _clauseService;
        private readonly ILogger<ClausesController> _logger;

        public ClausesController(
            IClauseService clauseService,
            ILogger<ClausesController> logger)
        {
            _clauseService = clauseService;
            _logger = logger;
        }

        /// <summary>
        /// Get all clauses with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetClauses([FromQuery] PagingParameters pagingParameters)
        {
            var response = await _clauseService.GetClausesAsync(pagingParameters);

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
        /// Get clause by id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetClauseById(long id)
        {
            var response = await _clauseService.GetClauseByIdAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Create a new clause
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateClause([FromBody] CreateClauseDto createDto)
        {
            var response = await _clauseService.CreateClauseAsync(createDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return CreatedAtAction(nameof(GetClauseById), new { id = response.Data.Id }, response);
        }

        /// <summary>
        /// Update an existing clause
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateClause(long id, [FromBody] UpdateClauseDto updateDto)
        {
            var response = await _clauseService.UpdateClauseAsync(id, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Delete a clause
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteClause(long id)
        {
            var response = await _clauseService.DeleteClauseAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Add checklist item to clause
        /// </summary>
        [HttpPost("{clauseId}/checklist")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddCheckListItem(long clauseId, [FromBody] CreateClauseCheckListDto createDto)
        {
            // Ensure the clauseId in the URL matches the one in the DTO
            if (clauseId != createDto.ClauseId)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("The clauseId in the URL must match the clauseId in the request body"));
            }

            var response = await _clauseService.AddCheckListItemAsync(createDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return CreatedAtAction(nameof(GetClauseById), new { id = clauseId }, response);
        }

        /// <summary>
        /// Update checklist item
        /// </summary>
        [HttpPut("checklist/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCheckListItem(long id, [FromBody] UpdateClauseCheckListDto updateDto)
        {
            var response = await _clauseService.UpdateCheckListItemAsync(id, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Delete checklist item
        /// </summary>
        [HttpDelete("checklist/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCheckListItem(long id)
        {
            var response = await _clauseService.DeleteCheckListItemAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }
    }
}