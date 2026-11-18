
using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Interfaces;

public interface IAuthService
{
    Task<User?> RegisterAsync(RegisterDto dto);

    Task<User?> LoginAsync(LoginDto dto);

    Task<bool> ChangePasswordAsync(
        int userId,
        ChangePasswordDto dto);

    Task RequestPasswordResetAsync(ForgotPasswordDto dto);

    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task<User?> VerifyMfaAsync(MfaVerifyDto dto);
}
