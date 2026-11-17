using Microsoft.EntityFrameworkCore;
namespace UserApi.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Age { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
    }
}
