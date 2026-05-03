using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService : MonoBehaviour
{
    private const string BaseUrl = "http://localhost:5270";

    public static ApiService Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GetHeroes(Action<HeroListResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(GetHeroesCoroutine(onSuccess, onError));
    }

    private IEnumerator GetHeroesCoroutine(Action<HeroListResponse> onSuccess, Action<string> onError)
    {
        using var request = UnityWebRequest.Get($"{BaseUrl}/api/heroes");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var response = JsonUtility.FromJson<HeroListResponse>(request.downloadHandler.text);
        onSuccess?.Invoke(response);
    }

    public void GetRunConfig(string heroId, Action<RunConfig> onSuccess, Action<string> onError)
    {
        StartCoroutine(GetRunConfigCoroutine(heroId, onSuccess, onError));
    }

    private IEnumerator GetRunConfigCoroutine(string heroId, Action<RunConfig> onSuccess, Action<string> onError)
    {
        using var request = UnityWebRequest.Get($"{BaseUrl}/api/run-config/{heroId}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var config = JsonUtility.FromJson<RunConfig>(request.downloadHandler.text);
        onSuccess?.Invoke(config);
    }

    public void GetMonsterMove(BattleStateRequest battleState, Action<Move> onSuccess, Action<string> onError)
    {
        StartCoroutine(GetMonsterMoveCoroutine(battleState, onSuccess, onError));
    }

    private IEnumerator GetMonsterMoveCoroutine(BattleStateRequest battleState, Action<Move> onSuccess, Action<string> onError)
    {
        var json = JsonUtility.ToJson(battleState);
        using var request = new UnityWebRequest($"{BaseUrl}/api/monster-move", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var move = JsonUtility.FromJson<Move>(request.downloadHandler.text);
        onSuccess?.Invoke(move);
    }
}
