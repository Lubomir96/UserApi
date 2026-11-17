
using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Interfaces;

public interface IAuthService
{
    Task<User?> RegisterAsync(RegisterDto dto);

    Task<User?> LoginAsync(LoginDto dto);
}
