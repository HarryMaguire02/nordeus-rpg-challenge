namespace NordeusChallenge.Api.Models;

public record LevelUpOption(string Label, Stats Stats);

public record HeroModel(
    string Id,
    string Name,
    string Description,
    Stats BaseStats,
    List<LevelUpOption> LevelUpOptions,
    int XpPerWin,
    int XpToLevelUp,
    List<string> DefaultMoveIds);
