namespace NordeusChallenge.Api.Models;

public record HeroModel(
    string Name,
    Stats BaseStats,
    Stats StatIncreasePerLevel,
    int XpPerWin,
    int XpToLevelUp,
    List<string> DefaultMoveIds);
