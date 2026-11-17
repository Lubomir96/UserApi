
using Microsoft.AspNetCore.Mvc;
using UserApi.DTOs;
using UserApi.Interfaces;
using UserApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace UserApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/user
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _userService.GetAllAsync();

        return Ok(users);
    }

    // GET: api/user/1
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // POST: api/user

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(CreateUserDto dto)
    {
        var emailExists = await _userService.EmailExistsAsync(dto.Email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "This email is already registered."
            });
        }

        var user = await _userService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            user
        );
    }


    // PUT: api/user/1

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        int id, UpdateUserDto dto)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
            return NotFound();

        var emailExists = await _userService.EmailExistsAsync(
            dto.Email, id);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "This email is already registered."
            });
        }

        await _userService.UpdateAsync(id, dto);

        return NoContent();
    }
    

    // DELETE: api/user/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
