using Application.DTOs.Clause;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IClauseService
    {
        Task<ApiResponse<PagedList<ClauseDto>>> GetClausesAsync(PagingParameters pagingParameters);
        Task<ApiResponse<ClauseDto>> GetClauseByIdAsync(long id);
        Task<ApiResponse<ClauseDto>> CreateClauseAsync(CreateClauseDto createDto);
        Task<ApiResponse<ClauseDto>> UpdateClauseAsync(long id, UpdateClauseDto updateDto);
        Task<ApiResponse<bool>> DeleteClauseAsync(long id);
        Task<ApiResponse<ClauseCheckListDto>> AddCheckListItemAsync(CreateClauseCheckListDto createDto);
        Task<ApiResponse<ClauseCheckListDto>> UpdateCheckListItemAsync(long id, UpdateClauseCheckListDto updateDto);
        Task<ApiResponse<bool>> DeleteCheckListItemAsync(long id);
    }
}
