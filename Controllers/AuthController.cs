
using Microsoft.AspNetCore.Mvc;
using UserApi.DTOs;
using UserApi.Interfaces;

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
        var user = await _authService.LoginAsync(dto);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
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
