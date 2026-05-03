namespace NordeusChallenge.Api.Dtos;

public record HeroConfigDto(
    string Id,
    string Name,
    string Description,
    StatsDto BaseStats,
    List<LevelUpOptionDto> LevelUpOptions,
    int XpPerWin,
    int XpToLevelUp,
    List<MoveDto> DefaultMoves);
