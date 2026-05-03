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

    [HttpGet("heroes")]
    public IActionResult GetHeroes()
    {
        return Ok(_gameDataService.GetHeroes());
    }

    [HttpGet("run-config/{heroId}")]
    public IActionResult GetRunConfig(string heroId)
    {
        var config = _gameDataService.GetRunConfig(heroId);
        if (config is null)
            return NotFound($"Hero '{heroId}' not found.");

        return Ok(config);
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
