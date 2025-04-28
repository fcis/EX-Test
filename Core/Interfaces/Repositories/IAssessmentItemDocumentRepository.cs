using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IAssessmentItemDocumentRepository : IGenericRepository<AssessmentItemDocument>
    {
        Task<IReadOnlyList<AssessmentItemDocument>> GetDocumentsByAssessmentItemAsync(long assessmentItemId);
        Task<PagedList<AssessmentItemDocument>> GetPagedDocumentsByAssessmentItemAsync(long assessmentItemId, PagingParameters pagingParameters);
    }
}
