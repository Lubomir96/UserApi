
using UserApi.DTOs;

namespace UserApi.Interfaces;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllAsync();

    Task<UserResponseDto?> GetByIdAsync(int id);

    Task<UserResponseDto> CreateAsync(CreateUserDto dto);

    Task<bool> UpdateAsync(int id, UpdateUserDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> EmailExistsAsync(
        string email,
        int? excludeUserId = null);
}
