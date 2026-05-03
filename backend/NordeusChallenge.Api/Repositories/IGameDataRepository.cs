using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Repositories;

public interface IGameDataRepository
{
    GameDataModel GetGameData();
}
