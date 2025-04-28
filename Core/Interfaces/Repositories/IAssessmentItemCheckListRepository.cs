using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IAssessmentItemCheckListRepository : IGenericRepository<AssessmentItemCheckList>
    {
        Task<IReadOnlyList<AssessmentItemCheckList>> GetCheckListItemsByAssessmentItemAsync(long assessmentItemId);
        Task<AssessmentItemCheckList?> GetByAssessmentItemAndCheckListAsync(long assessmentItemId, long checkListId);
    }
}
