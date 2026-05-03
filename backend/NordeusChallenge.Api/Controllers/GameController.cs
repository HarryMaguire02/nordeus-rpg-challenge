using Microsoft.AspNetCore.Mvc;
using NordeusChallenge.Api.Dtos;
using NordeusChallenge.Api.Services;

namespace NordeusChallenge.Api.Controllers;

[ApiController]
[Route("api")]
public class GameController : ControllerBase
{
    private readonly IGameDataService _gameDataService;

    public GameController(IGameDataService gameDataService)
    {
        _gameDataService = gameDataService;
    }

    [HttpGet("run-config")]
    public IActionResult GetRunConfig()
    {
        return Ok(_gameDataService.GetRunConfig());
    }

    [HttpPost("monster-move")]
    public IActionResult GetMonsterMove(BattleStateDto state)
    {
        var move = _gameDataService.GetMonsterMove(state);
        if (move is null)
        {
            return NotFound($"Monster '{state.MonsterId}' not found.");
        }

        return Ok(move);
    }
}
