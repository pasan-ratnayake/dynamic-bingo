using Microsoft.AspNetCore.Mvc;
using DynamicBingo.Application.Interfaces;
using DynamicBingo.Domain.Entities;

namespace DynamicBingo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LobbyController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IOpenChallengeRepository _challengeRepository;

    public LobbyController(IUserRepository userRepository, IOpenChallengeRepository challengeRepository)
    {
        _userRepository = userRepository;
        _challengeRepository = challengeRepository;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<User>>> GetLobbyUsers()
    {
        var users = await _userRepository.GetOnlineUsersAsync();
        return Ok(users);
    }

    [HttpGet("challenges")]
    public async Task<ActionResult<List<OpenChallenge>>> GetOpenChallenges()
    {
        var challenges = await _challengeRepository.GetActiveChallengesAsync();
        return Ok(challenges.ToList());
    }

    [HttpPost("challenges")]
    public async Task<ActionResult<OpenChallenge>> CreateChallenge([FromBody] CreateChallengeDto dto)
    {
        var challenge = OpenChallenge.Create(
            Guid.Parse(dto.CreatorId),
            dto.Visibility,
            dto.Word,
            dto.FillMode,
            dto.StarterChoice
        );

        await _challengeRepository.CreateAsync(challenge);
        return Ok(challenge);
    }

    [HttpPost("challenges/{id}/cancel")]
    public async Task<ActionResult> CancelChallenge(string id)
    {
        var challenge = await _challengeRepository.GetByIdAsync(Guid.Parse(id));
        if (challenge == null) return NotFound();

        challenge.Cancel();
        await _challengeRepository.UpdateAsync(challenge);
        return Ok();
    }

    [HttpPost("challenges/{id}/accept")]
    public async Task<ActionResult<AcceptChallengeDto>> AcceptChallenge(string id)
    {
        var challenge = await _challengeRepository.GetByIdAsync(Guid.Parse(id));
        if (challenge?.IsActive != true) return NotFound();

        var gameId = Guid.NewGuid().ToString();
        
        challenge.Cancel();
        await _challengeRepository.UpdateAsync(challenge);

        return Ok(new AcceptChallengeDto { GameId = gameId });
    }
}

public class CreateChallengeDto
{
    public string CreatorId { get; set; } = "";
    public Domain.Enums.ChallengeVisibility Visibility { get; set; }
    public string Word { get; set; } = "";
    public Domain.Enums.FillMode FillMode { get; set; }
    public Domain.Enums.StarterChoice StarterChoice { get; set; }
}

public class AcceptChallengeDto
{
    public string GameId { get; set; } = "";
}
