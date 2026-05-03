namespace NordeusChallenge.Api.Dtos;

public record BattleStateDto(
    string MonsterId,
    int MonsterCurrentHp,
    int MonsterMaxHp,
    int MonsterEffectiveAttack,
    int MonsterEffectiveDefense,
    int MonsterEffectiveMagic,
    int HeroCurrentHp,
    int HeroMaxHp,
    int HeroEffectiveAttack,
    int HeroEffectiveDefense,
    int HeroEffectiveMagic,
    int TurnNumber);
