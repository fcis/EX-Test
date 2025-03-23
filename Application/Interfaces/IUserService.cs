using Application.DTOs.User;
using Core.Common;
using Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserDto>> GetUserByIdAsync(long id);
        Task<ApiResponse<PagedList<UserListDto>>> GetUsersAsync(PagingParameters pagingParameters);
        Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request);
        Task<ApiResponse<UserDto>> UpdateUserAsync(long id, UpdateUserRequest request);
        Task<ApiResponse<bool>> DeleteUserAsync(long id);
        Task<ApiResponse<bool>> ActivateUserAsync(long id);
        Task<ApiResponse<bool>> DeactivateUserAsync(long id);
        Task<ApiResponse<PagedList<UserListDto>>> GetUsersByOrganizationAsync(long organizationId, PagingParameters pagingParameters);
    }
}
