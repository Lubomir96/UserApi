
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;
using UserApi.DTOs;
using UserApi.Interfaces;
using UserApi.Models;


namespace UserApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<User?> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var emailExists = await _userRepository.EmailExistsAsync(email);

        if (emailExists)
            return null;

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            Age = dto.Age
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            dto.Password);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user;
    }


    public async Task<User?> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                dto.Password);
        }

        // Ограничение: един MFA код на всеки 60 секунди
        if (user.MfaCodeLastSentAt.HasValue &&
            DateTime.UtcNow < user.MfaCodeLastSentAt.Value.AddSeconds(60))
        {
            throw new InvalidOperationException(
                "Please wait 60 seconds before requesting another MFA code.");
        }

        // Генериране на 6-цифрен код
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        // Запазване на SHA256 хеш
        user.MfaCodeHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(code)));

        // Валидност 5 минути
        user.MfaCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);

        // Нулиране на грешните опити
        user.MfaCodeAttempts = 0;

        // Записване на времето на изпращане
        user.MfaCodeLastSentAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();

        // Изпращане на кода по имейл
        var subject = "UserApi - MFA Verification Code";

        var body = $"""
        Hello {user.FirstName},

        Your login verification code is:

        {code}

        This code expires in 5 minutes.

        If you did not attempt to log in, please ignore this email.
        """;

        await _emailService.SendEmailAsync(
            user.Email,
            subject,
            body);

        return user;
    }


    public async Task<bool> ChangePasswordAsync(
        int userId,
        ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return false;

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.CurrentPassword);

        if (result == PasswordVerificationResult.Failed)
            return false;

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            dto.NewPassword);

        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email);

        // Не разкриваме дали даден имейл съществува.
        if (user == null)
            return;

        var randomBytes = RandomNumberGenerator.GetBytes(32);

        var token = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        user.PasswordResetTokenHash = tokenHash;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);

        await _userRepository.SaveChangesAsync();

        var subject = "UserApi - Password Reset";

        var body = $"""
            Hello {user.FirstName},

            We received a request to reset your password.

            Your password reset token is:

            {token}

            This token expires in 30 minutes.

            If you did not request this, you can ignore this email.
            """;

        await _emailService.SendEmailAsync(
            user.Email,
            subject,
            body);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            return false;

        if (string.IsNullOrWhiteSpace(user.PasswordResetTokenHash))
            return false;

        if (user.PasswordResetTokenExpiresAt == null ||
            user.PasswordResetTokenExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        var providedTokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(dto.Token)));

        if (!CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(user.PasswordResetTokenHash),
            Convert.FromHexString(providedTokenHash)))
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            dto.NewPassword);

        // Токенът се изтрива след успешно нулиране.
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAt = null;

        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<User?> VerifyMfaAsync(MfaVerifyDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            return null;

        if (string.IsNullOrWhiteSpace(user.MfaCodeHash) ||
            user.MfaCodeExpiresAt == null)
        {
            return null;
        }

        if (user.MfaCodeExpiresAt <= DateTime.UtcNow)
        {
            user.MfaCodeHash = null;
            user.MfaCodeExpiresAt = null;
            user.MfaCodeAttempts = 0;

            await _userRepository.SaveChangesAsync();

            return null;
        }

        if (user.MfaCodeAttempts >= 5)
            return null;

        var providedHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(dto.Code));

        var storedHash = Convert.FromHexString(user.MfaCodeHash);

        if (!CryptographicOperations.FixedTimeEquals(
            storedHash,
            providedHash))
        {
            user.MfaCodeAttempts++;

            if (user.MfaCodeAttempts >= 5)
            {
                user.MfaCodeHash = null;
                user.MfaCodeExpiresAt = null;
                user.MfaCodeAttempts = 0;
            }

            await _userRepository.SaveChangesAsync();

            return null;
        }

        // Успешна проверка – кодът се използва само веднъж.
        user.MfaCodeHash = null;
        user.MfaCodeExpiresAt = null;
        user.MfaCodeAttempts = 0;

        await _userRepository.SaveChangesAsync();

        return user;
    }

}
