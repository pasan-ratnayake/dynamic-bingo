using Microsoft.AspNetCore.Mvc;
using DynamicBingo.Application.Interfaces;
using DynamicBingo.Application.Services;
using DynamicBingo.Domain.Entities;

namespace DynamicBingo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameRepository _gameRepository;
    private readonly GameEngineService _gameEngine;

    public GamesController(IGameRepository gameRepository, GameEngineService gameEngine)
    {
        _gameRepository = gameRepository;
        _gameEngine = gameEngine;
    }

    [HttpGet("ongoing")]
    public async Task<ActionResult<List<Game>>> GetOngoingGames([FromQuery] string? userId)
    {
        if (string.IsNullOrEmpty(userId)) 
        {
            return Ok(new List<Game>());
        }
        
        var games = await _gameRepository.GetOngoingGamesForUserAsync(Guid.Parse(userId));
        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetGame(string id)
    {
        var game = await _gameRepository.GetByIdAsync(Guid.Parse(id));
        if (game == null) return NotFound();
        
        return Ok(game);
    }

    [HttpGet("{id}/state")]
    public async Task<ActionResult<object>> GetGameState(string id, [FromQuery] string userId)
    {
        if (string.IsNullOrEmpty(userId)) return BadRequest("UserId is required");
        
        var game = await _gameRepository.GetByIdAsync(Guid.Parse(id));
        if (game == null) return NotFound();
        
        return Ok(new { game });
    }
}
