using NordeusChallenge.Api.Dtos;

namespace NordeusChallenge.Api.Services;

public interface IGameDataService
{
    RunConfigDto GetRunConfig();
    MoveDto? GetMonsterMove(BattleStateDto state);
}
