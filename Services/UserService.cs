
using UserApi.DTOs;
using UserApi.Interfaces;
using UserApi.Models;

namespace UserApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // GET ALL USERS
    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users.Select(MapToResponse).ToList();
    }

    // GET USER BY ID
    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            return null;

        return MapToResponse(user);
    }

    // CREATE USER
    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email.Trim().ToLowerInvariant(),
            Age = dto.Age
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return MapToResponse(user);
    }

    // UPDATE USER
    public async Task<bool> UpdateAsync(
        int id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            return false;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email.Trim().ToLowerInvariant();
        user.Age = dto.Age;

        await _userRepository.SaveChangesAsync();

        return true;
    }

    // DELETE USER
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            return false;

        _userRepository.Delete(user);

        await _userRepository.SaveChangesAsync();

        return true;
    }

    // CHECK EMAIL
    public async Task<bool> EmailExistsAsync(
        string email,
        int? excludeUserId = null)
    {
        return await _userRepository.EmailExistsAsync(
            email, excludeUserId);
    }

    // MAP USER TO RESPONSE DTO
    private static UserResponseDto MapToResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Age = user.Age
        };
    }
}
