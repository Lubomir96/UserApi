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
        public string? MfaCodeHash { get; set; }
        public DateTime? MfaCodeExpiresAt { get; set; }
        public int MfaCodeAttempts { get; set; }
        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetTokenExpiresAt { get; set; }
        public DateTime? MfaCodeLastSentAt { get; set; }
    }
}
