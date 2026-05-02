using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        statusText.text = string.Empty;
        startButton.onClick.AddListener(OnStartClicked);
        GameManager.Instance.OnRunConfigLoadFailed += OnLoadFailed;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRunConfigLoadFailed -= OnLoadFailed;
    }

    private void OnStartClicked()
    {
        startButton.interactable = false;
        statusText.text = "Loading...";
        GameManager.Instance.StartRun();
    }

    private void OnLoadFailed(string error)
    {
        startButton.interactable = true;
        statusText.text = $"Error: {error}\nMake sure the server is running.";
    }
}
