using Core.Common;
using Core.Entities.Identitiy;
using Core.Interfaces.Authentication;
using Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsers([FromQuery] PagingParameters pagingParameters)
        {
            var response = await _userService.GetUsersAsync(pagingParameters);

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
        /// Get user by id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserById(long id)
        {
            var response = await _userService.GetUserByIdAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest createRequest)
        {
            var response = await _userService.CreateUserAsync(createRequest);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return CreatedAtAction(nameof(GetUserById), new { id = response.Data.Id }, response);
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserRequest updateRequest)
        {
            var response = await _userService.UpdateUserAsync(id, updateRequest);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var response = await _userService.DeleteUserAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Activate a user
        /// </summary>
        [HttpPatch("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ActivateUser(long id)
        {
            var response = await _userService.ActivateUserAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Deactivate a user
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeactivateUser(long id)
        {
            var response = await _userService.DeactivateUserAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Get users by organization
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsersByOrganization(long organizationId, [FromQuery] PagingParameters pagingParameters)
        {
            var response = await _userService.GetUsersByOrganizationAsync(organizationId, pagingParameters);

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
    }
}