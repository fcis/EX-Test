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
    public interface IAssessmentRepository : IGenericRepository<Assessment>
    {
        Task<Assessment?> GetAssessmentWithItemsAsync(long assessmentId);
        Task<Assessment?> GetAssessmentByOrganizationAndFrameworkVersionAsync(long organizationId, long frameworkVersionId);
        Task<PagedList<Assessment>> GetAssessmentsByOrganizationAsync(long organizationId, PagingParameters pagingParameters);
        Task<PagedList<Assessment>> GetAssessmentsByStatusAsync(AssessmentStatus status, PagingParameters pagingParameters);
        Task<List<Assessment>> GetAssessmentsNeedingNotificationAsync();
        Task<int> GetAssessmentCompletionPercentageAsync(long assessmentId);
        Task<IReadOnlyList<Assessment>> GetRecentAssessmentsAsync(int count);
    }
}
