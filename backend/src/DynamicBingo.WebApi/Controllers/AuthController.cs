using DynamicBingo.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DynamicBingo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("magic-links")]
    public async Task<IActionResult> SendMagicLink([FromBody] SendMagicLinkRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var success = await _authService.SendMagicLinkAsync(request.Email, ipAddress, userAgent);
        
        if (!success)
            return BadRequest("Unable to send magic link");
            
        return NoContent();
    }

    [HttpPost("magic-links/consume")]
    public async Task<IActionResult> ConsumeMagicLink([FromBody] ConsumeMagicLinkRequest request)
    {
        var user = await _authService.ConsumeMagicLinkAsync(request.Token);
        
        if (user == null)
            return BadRequest("Invalid or expired token");
            
        return Ok(new
        {
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsGuest
        });
    }

    [HttpPost("guests")]
    public async Task<IActionResult> CreateGuest([FromBody] CreateGuestRequest request)
    {
        try
        {
            var user = await _authService.CreateGuestAsync(request.DisplayName);
            return Ok(new
            {
                user.Id,
                user.DisplayName,
                user.IsGuest
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("guests/convert")]
    public async Task<IActionResult> ConvertGuest([FromBody] ConvertGuestRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var success = await _authService.ConvertGuestToRegisteredAsync(request.GuestId, request.Email, ipAddress, userAgent);
        
        if (!success)
            return BadRequest("Unable to convert guest account");
            
        return NoContent();
    }

    [HttpPost("guests/convert/complete")]
    public async Task<IActionResult> CompleteGuestConversion([FromBody] CompleteGuestConversionRequest request)
    {
        var user = await _authService.CompleteGuestConversionAsync(request.Token, request.Email);
        
        if (user == null)
            return BadRequest("Invalid token or email");
            
        return Ok(new
        {
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsGuest
        });
    }
}

public record SendMagicLinkRequest(string Email);
public record ConsumeMagicLinkRequest(string Token);
public record CreateGuestRequest(string DisplayName);
public record ConvertGuestRequest(Guid GuestId, string Email);
public record CompleteGuestConversionRequest(string Token, string Email);
