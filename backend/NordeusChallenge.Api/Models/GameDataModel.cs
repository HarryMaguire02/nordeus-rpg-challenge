namespace NordeusChallenge.Api.Models;

public record GameDataModel(HeroModel Hero, List<Move> Moves, List<MonsterModel> Monsters);
