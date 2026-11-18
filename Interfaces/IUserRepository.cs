using UserApi.Models;

namespace UserApi.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();

        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);

        Task AddAsync(User user);

        void Delete(User user);

        Task<bool> EmailExistsAsync(
            string email,
            int? excludeUserId = null);

        Task SaveChangesAsync();
    }
}
