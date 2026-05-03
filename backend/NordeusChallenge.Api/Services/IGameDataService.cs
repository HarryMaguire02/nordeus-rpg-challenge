using NordeusChallenge.Api.Dtos;

namespace NordeusChallenge.Api.Services;

public interface IGameDataService
{
    HeroListDto GetHeroes();
    RunConfigDto? GetRunConfig(string heroId);
    MoveDto? GetMonsterMove(BattleStateDto state);
}
