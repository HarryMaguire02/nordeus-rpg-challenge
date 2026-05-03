namespace NordeusChallenge.Api.Dtos;

public record HeroConfigDto(
    string Name,
    StatsDto BaseStats,
    StatsDto StatIncreasePerLevel,
    int XpPerWin,
    int XpToLevelUp,
    List<MoveDto> DefaultMoves);
