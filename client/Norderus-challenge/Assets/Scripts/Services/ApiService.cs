using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService : MonoBehaviour
{
    private const string BaseUrl = "http://localhost:5000";

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

    public void GetRunConfig(Action<RunConfig> onSuccess, Action<string> onError)
    {
        StartCoroutine(GetRunConfigCoroutine(onSuccess, onError));
    }

    private IEnumerator GetRunConfigCoroutine(Action<RunConfig> onSuccess, Action<string> onError)
    {
        using var request = UnityWebRequest.Get($"{BaseUrl}/api/run-config");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var config = JsonUtility.FromJson<RunConfig>(request.downloadHandler.text);
        onSuccess?.Invoke(config);
    }

    public void GetMonsterMove(BattleStateRequest battleState, Action<MonsterMoveResponse> onSuccess, Action<string> onError)
    {
        StartCoroutine(GetMonsterMoveCoroutine(battleState, onSuccess, onError));
    }

    private IEnumerator GetMonsterMoveCoroutine(BattleStateRequest battleState, Action<MonsterMoveResponse> onSuccess, Action<string> onError)
    {
        var json = JsonUtility.ToJson(battleState);
        using var request = new UnityWebRequest($"{BaseUrl}/api/monster-move", "POST");
        request.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var response = JsonUtility.FromJson<MonsterMoveResponse>(request.downloadHandler.text);
        onSuccess?.Invoke(response);
    }
}
