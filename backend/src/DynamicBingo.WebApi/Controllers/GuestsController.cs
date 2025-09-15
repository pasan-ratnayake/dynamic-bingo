using DynamicBingo.Application.Services;
using DynamicBingo.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DynamicBingo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestsController : ControllerBase
{
    private readonly AuthService _authService;

    public GuestsController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateGuest([FromBody] CreateGuestRequest request)
    {
        try
        {
            var (user, token) = await _authService.CreateGuestWithTokenAsync(request.DisplayName);
            
            return Ok(new
            {
                user = new
                {
                    id = user.Id,
                    displayName = user.DisplayName,
                    isGuest = user.IsGuest,
                    email = user.Email,
                    createdAt = user.CreatedAt,
                    lastActiveAt = user.LastActiveAt
                },
                token = token
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("convert")]
    public async Task<ActionResult> ConvertGuest([FromBody] ConvertGuestRequest request)
    {
        try
        {
            await _authService.SendMagicLinkAsync(request.Email);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record CreateGuestRequest(string DisplayName);
public record ConvertGuestRequest(string Email);
