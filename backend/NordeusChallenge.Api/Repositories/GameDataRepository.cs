using System.Text.Json;
using System.Text.Json.Serialization;
using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Repositories;

// To swap to a real database, implement IGameDataRepository against your data source
// and update the DI binding in Program.cs — no other code changes required.
public class GameDataRepository : IGameDataRepository
{
    private readonly Lazy<GameDataModel> _data;

    public GameDataRepository(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "game-data.json");
        _data = new Lazy<GameDataModel>(() => Load(path));
    }

    public GameDataModel GetGameData() => _data.Value;

    private static GameDataModel Load(string path)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Deserialize<GameDataModel>(File.ReadAllText(path), options)
            ?? throw new InvalidOperationException("Failed to load game-data.json.");
    }
}
