
using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<User> CreateAsync(CreateUserDto dto);

    Task<bool> UpdateAsync(int id, UpdateUserDto dto);

    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
}
