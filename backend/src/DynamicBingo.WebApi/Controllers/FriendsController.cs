using Microsoft.AspNetCore.Mvc;
using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;
using DynamicBingo.Domain.Enums;

namespace DynamicBingo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FriendsController : ControllerBase
{
    private readonly IFriendshipRepository _friendshipRepository;

    public FriendsController(IFriendshipRepository friendshipRepository)
    {
        _friendshipRepository = friendshipRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Friendship>>> GetFriends([FromQuery] string? userId)
    {
        if (string.IsNullOrEmpty(userId)) 
        {
            return Ok(new List<Friendship>());
        }
        
        var friendships = await _friendshipRepository.GetFriendshipsForUserAsync(Guid.Parse(userId));
        return Ok(friendships);
    }

    [HttpPost("requests")]
    public async Task<ActionResult<Friendship>> SendFriendRequest([FromBody] SendFriendRequestDto dto)
    {
        var existingFriendships = await _friendshipRepository.GetFriendshipsForUserAsync(Guid.Parse(dto.FromUserId));
        if (existingFriendships?.Any(f => f.UserAId == Guid.Parse(dto.ToUserId) || f.UserBId == Guid.Parse(dto.ToUserId)) == true) 
            return Conflict("Friendship already exists");

        var friendship = Friendship.Create(Guid.Parse(dto.FromUserId), Guid.Parse(dto.ToUserId));

        await _friendshipRepository.CreateAsync(friendship);
        return Ok(friendship);
    }

    [HttpPost("requests/{id}/respond")]
    public async Task<ActionResult> RespondToFriendRequest(string id, [FromBody] RespondFriendRequestDto dto)
    {
        var friendship = await _friendshipRepository.GetByIdAsync(Guid.Parse(id));
        if (friendship == null) return NotFound();

        if (dto.Accept)
        {
            friendship.Accept();
        }
        else
        {
            friendship.Block();
        }

        await _friendshipRepository.UpdateAsync(friendship);
        return Ok();
    }
}

public class SendFriendRequestDto
{
    public string FromUserId { get; set; } = "";
    public string ToUserId { get; set; } = "";
}

public class RespondFriendRequestDto
{
    public bool Accept { get; set; }
}
