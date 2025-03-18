using Application.DTOs.Framework;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFrameworkService
    {
        Task<ApiResponse<PagedList<FrameworkListDto>>> GetFrameworksAsync(PagingParameters pagingParameters);
        Task<ApiResponse<FrameworkDto>> GetFrameworkByIdAsync(long id);
        Task<ApiResponse<FrameworkDto>> CreateFrameworkAsync(CreateFrameworkDto createDto);
        Task<ApiResponse<FrameworkDto>> UpdateFrameworkAsync(long id, UpdateFrameworkDto updateDto);
        Task<ApiResponse<bool>> DeleteFrameworkAsync(long id);
        Task<ApiResponse<FrameworkVersionDto>> AddVersionAsync(CreateFrameworkVersionDto createDto);
        Task<ApiResponse<FrameworkVersionDto>> UpdateVersionAsync(long id, UpdateFrameworkVersionDto updateDto);
        Task<ApiResponse<bool>> DeleteVersionAsync(long id);
        Task<ApiResponse<bool>> PublishFrameworkAsync(long id);
    }
}
