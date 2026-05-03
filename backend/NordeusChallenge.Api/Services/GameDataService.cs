using AutoMapper;
using NordeusChallenge.Api.Dtos;
using NordeusChallenge.Api.Models;
using NordeusChallenge.Api.Repositories;

namespace NordeusChallenge.Api.Services;

public class GameDataService : IGameDataService
{
    private readonly IGameDataRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMonsterMoveEngine _moveEngine;

    public GameDataService(IGameDataRepository repository, IMapper mapper, IMonsterMoveEngine moveEngine)
    {
        _repository = repository;
        _mapper     = mapper;
        _moveEngine = moveEngine;
    }

    public HeroListDto GetHeroes()
    {
        var data = _repository.GetGameData();
        var summaries = data.Heroes.Select(h => new HeroSummaryDto(
            h.Id,
            h.Name,
            h.Description,
            _mapper.Map<StatsDto>(h.BaseStats)
        )).ToList();
        return new HeroListDto(summaries);
    }

    public RunConfigDto? GetRunConfig(string heroId)
    {
        var data = _repository.GetGameData();
        var hero = data.Heroes.FirstOrDefault(h => h.Id == heroId);
        if (hero is null) return null;

        var movesById = BuildMovesById(data);
        var monsters = BuildMonsterDtos(data, movesById);

        var heroDto = new HeroConfigDto(
            hero.Id,
            hero.Name,
            hero.Description,
            _mapper.Map<StatsDto>(hero.BaseStats),
            _mapper.Map<List<LevelUpOptionDto>>(hero.LevelUpOptions),
            hero.XpPerWin,
            hero.XpToLevelUp,
            hero.DefaultMoveIds.Select(id => movesById[id]).ToList()
        );

        return new RunConfigDto(heroDto, monsters);
    }

    public MoveDto? GetMonsterMove(BattleStateDto state)
    {
        var data = _repository.GetGameData();
        var monster = data.Monsters.FirstOrDefault(m => m.Id == state.MonsterId);
        if (monster is null) return null;

        var movesById = BuildMovesById(data);
        var monsterDto = BuildMonsterDto(monster, movesById);

        return _moveEngine.PickMove(monsterDto, state);
    }

    private Dictionary<string, MoveDto> BuildMovesById(GameDataModel data) =>
        data.Moves.ToDictionary(m => m.Id, m => _mapper.Map<MoveDto>(m));

    private MonsterConfigDto BuildMonsterDto(MonsterModel monster, Dictionary<string, MoveDto> movesById) =>
        new MonsterConfigDto(
            monster.Id,
            monster.Name,
            _mapper.Map<StatsDto>(monster.Stats),
            monster.MoveIds.Select(id => movesById[id]).ToList()
        );

    private List<MonsterConfigDto> BuildMonsterDtos(GameDataModel data, Dictionary<string, MoveDto> movesById) =>
        data.Monsters.Select(m => BuildMonsterDto(m, movesById)).ToList();
}
