using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Dtos;

public record MoveDto(string Id, string Name, MoveType Type, string Description, MoveEffectDto Primary, MoveEffectDto? Secondary);
