using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Dtos;

public record MoveEffectDto(EffectKind Kind, int BaseValue, int Duration);
