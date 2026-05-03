using AutoMapper;
using NordeusChallenge.Api.Dtos;
using NordeusChallenge.Api.Repositories;

namespace NordeusChallenge.Api.Services;

public class GameDataService : IGameDataService
{
    private readonly IGameDataRepository _repository;
    private readonly IMapper _mapper;

    public GameDataService(IGameDataRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public RunConfigDto GetRunConfig()
    {
        var data = _repository.GetGameData();
        var movesById = data.Moves.ToDictionary(m => m.Id, m => _mapper.Map<MoveDto>(m));

        var monsters = data.Monsters.Select(m => new MonsterConfigDto(
            m.Id,
            m.Name,
            _mapper.Map<StatsDto>(m.Stats),
            m.MoveIds.Select(id => movesById[id]).ToList()
        )).ToList();

        var hero = new HeroConfigDto(
            data.Hero.Name,
            _mapper.Map<StatsDto>(data.Hero.BaseStats),
            _mapper.Map<StatsDto>(data.Hero.StatIncreasePerLevel),
            data.Hero.XpPerWin,
            data.Hero.XpToLevelUp,
            data.Hero.DefaultMoveIds.Select(id => movesById[id]).ToList()
        );

        return new RunConfigDto(hero, monsters);
    }

    public MoveDto? GetMonsterMove(BattleStateDto state)
    {
        var data = _repository.GetGameData();
        var monster = data.Monsters.FirstOrDefault(m => m.Id == state.MonsterId);
        if (monster is null)
        {
            return null;
        }

        var movesById = data.Moves.ToDictionary(m => m.Id, m => _mapper.Map<MoveDto>(m));
        var monsterDto = new MonsterConfigDto(
            monster.Id,
            monster.Name,
            _mapper.Map<StatsDto>(monster.Stats),
            monster.MoveIds.Select(id => movesById[id]).ToList()
        );

        return MonsterSmartMoveEngine.PickMove(monsterDto, state);
    }
}
