
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.DTOs;
using UserApi.Interfaces;
using UserApi.Models;

namespace UserApi.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> CreateAsync(CreateUserDto dto)
    {
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email.Trim().ToLowerInvariant(),
            Age = dto.Age
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return false;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email.Trim().ToLowerInvariant();
        user.Age = dto.Age;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return false;

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        int? excludeUserId = null)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var query = _context.Users
            .Where(u => u.Email == normalizedEmail);

        if (excludeUserId.HasValue)
        {
            query = query.Where(
                u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }
    
}
