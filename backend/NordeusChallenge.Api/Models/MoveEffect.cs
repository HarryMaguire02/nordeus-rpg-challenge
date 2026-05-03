namespace NordeusChallenge.Api.Models;

public record MoveEffect(EffectKind Kind, int BaseValue, int Duration = 0);
