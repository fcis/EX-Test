using Core.Common;
using Core.Entities;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IAssessmentItemRepository : IGenericRepository<AssessmentItem>
    {
        Task<AssessmentItem?> GetAssessmentItemWithDetailsAsync(long assessmentItemId);
        Task<PagedList<AssessmentItem>> GetAssessmentItemsByAssessmentAsync(long assessmentId, PagingParameters pagingParameters);
        Task<PagedList<AssessmentItem>> GetAssessmentItemsByStatusAsync(long assessmentId, ComplianceStatus status, PagingParameters pagingParameters);
        Task<PagedList<AssessmentItem>> GetAssessmentItemsByDepartmentAsync(long assessmentId, long departmentId, PagingParameters pagingParameters);
        Task<Dictionary<ComplianceStatus, int>> GetStatusSummaryAsync(long assessmentId);
    }
}
