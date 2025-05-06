using Application.DTOs.Assessment;
using Core.Common;
using Core.Enums;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAssessmentService
    {
        // Assessment Management
        Task<ApiResponse<PagedList<AssessmentListDto>>> GetAssessmentsAsync(PagingParameters pagingParameters);
        Task<ApiResponse<PagedList<AssessmentListDto>>> GetAssessmentsByOrganizationAsync(long organizationId, PagingParameters pagingParameters);
        Task<ApiResponse<AssessmentDto>> GetAssessmentByIdAsync(long id);
        Task<ApiResponse<AssessmentDto>> StartAssessmentAsync(long organizationMembershipId, string? notes = null);
        Task<ApiResponse<AssessmentDto>> UpdateAssessmentAsync(long id, UpdateAssessmentDto updateDto);
        Task<ApiResponse<bool>> DeleteAssessmentAsync(long id);

        // Assessment Item Management
        Task<ApiResponse<PagedList<AssessmentItemDto>>> GetAssessmentItemsAsync(long assessmentId, PagingParameters pagingParameters);
        Task<ApiResponse<AssessmentItemDto>> GetAssessmentItemByIdAsync(long id);
        Task<ApiResponse<AssessmentItemDto>> UpdateAssessmentItemAsync(long id, UpdateAssessmentItemDto updateDto);

        // Document Management
        Task<ApiResponse<AssessmentItemDocumentDto>> UploadDocumentAsync(UploadAssessmentDocumentDto uploadDto);
        Task<ApiResponse<bool>> DeleteDocumentAsync(long id);
        Task<ApiResponse<byte[]>> DownloadDocumentAsync(long id);

        // Checklist Management
        Task<ApiResponse<AssessmentItemCheckListDto>> UpdateCheckListItemAsync(long assessmentItemId, UpdateAssessmentItemCheckListDto updateDto);

        // Reporting
        Task<ApiResponse<GapAnalysisDto>> GenerateGapAnalysisAsync(long assessmentId);
        Task<ApiResponse<GapAnalysisDto>> GenerateGapAnalysisByDepartmentAsync(long assessmentId, long departmentId);
        Task<ApiResponse<GapAnalysisDto>> GenerateGapAnalysisByCategoryAsync(long assessmentId, long categoryId);

        // Statistics and Dashboard Data
        Task<ApiResponse<Dictionary<string, int>>> GetAssessmentStatusSummaryAsync(long organizationId);
        Task<ApiResponse<Dictionary<string, int>>> GetComplianceStatusSummaryAsync(long assessmentId);
    }
}