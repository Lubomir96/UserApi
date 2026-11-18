
using Microsoft.AspNetCore.Mvc;
using UserApi.DTOs;
using UserApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = await _authService.RegisterAsync(dto);

        if (user == null)
        {
            return Conflict(new
            {
                message = "Email is already registered."
            });
        }

        return StatusCode(201, new
        {
            message = "Registration successful!",
            user = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Age
            }
        });
    }

    // POST: api/auth/login

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            var user = await _authService.LoginAsync(dto);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            return Ok(new
            {
                message = "Verification code sent to your email.",
                requiresMfa = true
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(429, new
            {
                message = ex.Message
            });
        }
    }



    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId, dto);

        if (!result)
            return BadRequest(new
            {
                message = "Current password is incorrect."
            });

        return Ok(new
        {
            message = "Password changed successfully."
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        await _authService.RequestPasswordResetAsync(dto);

        return Ok(new
        {
            message = "If the email exists, a password reset token has been sent."
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);

        if (!result)
        {
            return BadRequest(new
            {
                message = "Invalid or expired password reset token."
            });
        }

        return Ok(new
        {
            message = "Password has been reset successfully."
        });
    }


    [HttpPost("verify-mfa")]
    public async Task<IActionResult> VerifyMfa(MfaVerifyDto dto)
    {
        var user = await _authService.VerifyMfaAsync(dto);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid or expired verification code."
            });
        }

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            message = "Login successful!",
            token,
            tokenType = "Bearer",
            expiresIn = 7200
        });
    }



}
