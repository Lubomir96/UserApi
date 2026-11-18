
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Interfaces;
using UserApi.Models;

namespace UserApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
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


    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
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

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
