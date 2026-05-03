namespace NordeusChallenge.Api.Dtos;

public record MonsterConfigDto(string Id, string Name, StatsDto Stats, List<MoveDto> Moves);
