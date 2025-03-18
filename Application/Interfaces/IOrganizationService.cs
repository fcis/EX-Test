using Application.DTOs.Organization;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrganizationService
    {
        Task<ApiResponse<PagedList<OrganizationListDto>>> GetOrganizationsAsync(PagingParameters pagingParameters);
        Task<ApiResponse<OrganizationDto>> GetOrganizationByIdAsync(long id);
        Task<ApiResponse<OrganizationDto>> CreateOrganizationAsync(CreateOrganizationDto createDto);
        Task<ApiResponse<OrganizationDto>> UpdateOrganizationAsync(long id, UpdateOrganizationDto updateDto);
        Task<ApiResponse<bool>> DeleteOrganizationAsync(long id);
        Task<ApiResponse<OrganizationDepartmentDto>> AddDepartmentAsync(long organizationId, CreateOrganizationDepartmentDto createDto);
        Task<ApiResponse<OrganizationDepartmentDto>> UpdateDepartmentAsync(long id, UpdateOrganizationDepartmentDto updateDto);
        Task<ApiResponse<bool>> DeleteDepartmentAsync(long id);
        Task<ApiResponse<bool>> AssignFrameworkAsync(long organizationId, long frameworkId, long frameworkVersionId);
    }
}
