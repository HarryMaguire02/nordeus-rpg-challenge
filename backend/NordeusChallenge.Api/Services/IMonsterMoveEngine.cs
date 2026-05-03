using NordeusChallenge.Api.Dtos;

namespace NordeusChallenge.Api.Services;

public interface IMonsterMoveEngine
{
    MoveDto PickMove(MonsterConfigDto monster, BattleStateDto state);
}
