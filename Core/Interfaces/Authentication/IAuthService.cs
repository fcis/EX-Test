using Core.Common;
using Core.Models.Auth;
using Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Authentication
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ApiResponse<AuthResponse>> GetCurrentUserByTokenAsync(string token);

        Task<ApiResponse<bool>> LogoutAsync(string username);
        Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
    }
}
