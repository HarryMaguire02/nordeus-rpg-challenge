using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        statusText.text = string.Empty;

        startButton.onClick.AddListener(() => SceneManager.LoadScene("CharacterSelect"));

        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(GameManager.HasSave());
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        exitButton.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });

        GameManager.Instance.OnRunConfigLoadFailed += OnLoadFailed;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRunConfigLoadFailed -= OnLoadFailed;
    }

    private void OnResumeClicked()
    {
        startButton.interactable = false;
        resumeButton.interactable = false;
        statusText.text = "Loading saved run...";
        GameManager.Instance.ResumeRun();
    }

    private void OnLoadFailed(string error)
    {
        startButton.interactable = true;
        if (resumeButton != null) resumeButton.interactable = true;
        statusText.text = $"Error: {error}\nMake sure the server is running.";
    }
}
