using DynamicBingo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicBingo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        return Ok(new { Message = "User endpoint - authentication not implemented yet" });
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        return Ok(new { Message = "Update user endpoint - authentication not implemented yet" });
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe()
    {
        return Ok(new { Message = "Delete user endpoint - authentication not implemented yet" });
    }

    [HttpGet("online")]
    public async Task<IActionResult> GetOnlineUsers()
    {
        var users = await _userRepository.GetOnlineUsersAsync();
        return Ok(users.Select(u => new
        {
            u.Id,
            u.DisplayName,
            u.IsGuest
        }));
    }
}

public record UpdateUserRequest(string? DisplayName, string? Email);
