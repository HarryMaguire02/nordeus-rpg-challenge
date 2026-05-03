namespace NordeusChallenge.Api.Models;

public record Move(string Id, string Name, MoveType Type, string Description, MoveEffect Primary, MoveEffect? Secondary = null);
