using System.Text.Json;
using System.Text.Json.Serialization;
using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Repositories;

// To swap to a real database, implement IGameDataRepository against your data source
// and update the DI binding in Program.cs — no other code changes required.
public class GameDataRepository : IGameDataRepository
{
    private readonly string _dataFilePath;

    public GameDataRepository(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.ContentRootPath, "Data", "game-data.json");
    }

    public GameDataModel GetGameData()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Deserialize<GameDataModel>(File.ReadAllText(_dataFilePath), options)
            ?? throw new InvalidOperationException("Failed to load game-data.json.");
    }
}
