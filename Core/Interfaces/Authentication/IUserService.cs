using Core.Common;
using Core.Entities.Identitiy;
using Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Authentication
{
    public interface IUserService
    {
        Task<ApiResponse<User>> GetUserByIdAsync(long id);
        Task<ApiResponse<PagedList<User>>> GetUsersAsync(PagingParameters pagingParameters);
        Task<ApiResponse<User>> CreateUserAsync(CreateUserRequest request);
        Task<ApiResponse<User>> UpdateUserAsync(long id, UpdateUserRequest request);
        Task<ApiResponse<bool>> DeleteUserAsync(long id);
        Task<ApiResponse<bool>> ActivateUserAsync(long id);
        Task<ApiResponse<bool>> DeactivateUserAsync(long id);
        Task<ApiResponse<PagedList<User>>> GetUsersByOrganizationAsync(long organizationId, PagingParameters pagingParameters);
    }
}
