using Application.DTOs.Assessment;
using Application.Interfaces;
using Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssessmentsController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<AssessmentsController> _logger;

        public AssessmentsController(
            IAssessmentService assessmentService,
            ILogger<AssessmentsController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all assessments with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAssessments([FromQuery] PagingParameters pagingParameters)
        {
            var response = await _assessmentService.GetAssessmentsAsync(pagingParameters);

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
        /// Get assessments by organization
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAssessmentsByOrganization(long organizationId, [FromQuery] PagingParameters pagingParameters)
        {
            var response = await _assessmentService.GetAssessmentsByOrganizationAsync(organizationId, pagingParameters);

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
        /// Get assessment by id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAssessmentById(long id)
        {
            var response = await _assessmentService.GetAssessmentByIdAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Create a new assessment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDto createDto)
        {
            var response = await _assessmentService.CreateAssessmentAsync(createDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return CreatedAtAction(nameof(GetAssessmentById), new { id = response.Data.Id }, response);
        }

        /// <summary>
        /// Update an existing assessment
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAssessment(long id, [FromBody] UpdateAssessmentDto updateDto)
        {
            var response = await _assessmentService.UpdateAssessmentAsync(id, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Delete an assessment
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteAssessment(long id)
        {
            var response = await _assessmentService.DeleteAssessmentAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Get assessment items with pagination
        /// </summary>
        [HttpGet("{assessmentId}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAssessmentItems(long assessmentId, [FromQuery] PagingParameters pagingParameters)
        {
            var response = await _assessmentService.GetAssessmentItemsAsync(assessmentId, pagingParameters);

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
        /// Get assessment item by id
        /// </summary>
        [HttpGet("items/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAssessmentItemById(long id)
        {
            var response = await _assessmentService.GetAssessmentItemByIdAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Update an assessment item
        /// </summary>
        [HttpPut("items/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAssessmentItem(long id, [FromBody] UpdateAssessmentItemDto updateDto)
        {
            var response = await _assessmentService.UpdateAssessmentItemAsync(id, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Upload document for an assessment item
        /// </summary>
        [HttpPost("documents")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadDocument([FromForm] UploadAssessmentDocumentDto uploadDto)
        {
            var response = await _assessmentService.UploadDocumentAsync(uploadDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return CreatedAtAction(nameof(GetAssessmentItemById), new { id = uploadDto.AssessmentItemId }, response);
        }

        /// <summary>
        /// Download document
        /// </summary>
        [HttpGet("documents/{id}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DownloadDocument(long id)
        {
            var response = await _assessmentService.DownloadDocumentAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            // Get the document details to determine content type and filename
            var document = await _assessmentService.GetAssessmentItemByIdAsync(id);
            if (document == null || !document.Success)
                return StatusCode(404, ApiResponse<string>.ErrorResponse("Document not found", 404));

            // Return the file
            return File(response.Data, "application/octet-stream");
        }

        /// <summary>
        /// Delete document
        /// </summary>
        [HttpDelete("documents/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteDocument(long id)
        {
            var response = await _assessmentService.DeleteDocumentAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Update checklist item
        /// </summary>
        [HttpPut("items/{assessmentItemId}/checklist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCheckListItem(long assessmentItemId, [FromBody] UpdateAssessmentItemCheckListDto updateDto)
        {
            var response = await _assessmentService.UpdateCheckListItemAsync(assessmentItemId, updateDto);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Generate gap analysis for an assessment
        /// </summary>
        [HttpGet("{id}/gap-analysis")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateGapAnalysis(long id)
        {
            var response = await _assessmentService.GenerateGapAnalysisAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Generate gap analysis by department
        /// </summary>
        [HttpGet("{id}/gap-analysis/department/{departmentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateGapAnalysisByDepartment(long id, long departmentId)
        {
            var response = await _assessmentService.GenerateGapAnalysisByDepartmentAsync(id, departmentId);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Generate gap analysis by category
        /// </summary>
        [HttpGet("{id}/gap-analysis/category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateGapAnalysisByCategory(long id, long categoryId)
        {
            var response = await _assessmentService.GenerateGapAnalysisByCategoryAsync(id, categoryId);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Get assessment status summary for an organization
        /// </summary>
        [HttpGet("organization/{organizationId}/status-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAssessmentStatusSummary(long organizationId)
        {
            var response = await _assessmentService.GetAssessmentStatusSummaryAsync(organizationId);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }

        /// <summary>
        /// Get compliance status summary for an assessment
        /// </summary>
        [HttpGet("{id}/compliance-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetComplianceStatusSummary(long id)
        {
            var response = await _assessmentService.GetComplianceStatusSummaryAsync(id);

            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }
    }
}