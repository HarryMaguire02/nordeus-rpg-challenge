namespace NordeusChallenge.Api.Models;

public record MonsterModel(string Id, string Name, Stats Stats, List<string> MoveIds);
