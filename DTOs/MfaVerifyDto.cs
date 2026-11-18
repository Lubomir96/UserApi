
using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs;

public class MfaVerifyDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{6}$")]
    public string Code { get; set; } = string.Empty;
}
