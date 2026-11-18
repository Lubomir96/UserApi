using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}