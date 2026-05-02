using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("Monster")]
    [SerializeField] private TextMeshProUGUI monsterNameText;
    [SerializeField] private Slider monsterHpBar;
    [SerializeField] private TextMeshProUGUI monsterHpText;

    [Header("Hero")]
    [SerializeField] private Slider heroHpBar;
    [SerializeField] private TextMeshProUGUI heroHpText;

    [Header("Moves")]
    [SerializeField] private Button[] moveButtons;
    [SerializeField] private TextMeshProUGUI[] moveButtonTexts;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Win")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Button continueButton;

    [Header("Loss")]
    [SerializeField] private GameObject lossPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button mainMenuButton;

    private void Start()
    {
        winPanel.SetActive(false);
        lossPanel.SetActive(false);
        feedbackText.text = string.Empty;

        BattleManager.Instance.OnStatsUpdated    += OnStatsUpdated;
        BattleManager.Instance.OnMoveExecuted    += OnMoveExecuted;
        BattleManager.Instance.OnPlayerTurnStart += OnPlayerTurnStart;
        BattleManager.Instance.OnMonsterTurnStart += OnMonsterTurnStart;
        BattleManager.Instance.OnBattleEnd       += OnBattleEnd;

        continueButton.onClick.AddListener(() => GameManager.Instance.OnBattleWon());
        retryButton.onClick.AddListener(()    => GameManager.Instance.RetryBattle());
        mapButton.onClick.AddListener(()      => GameManager.Instance.ReturnToMap());
        mainMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMainMenu());

        // Must be last — StartBattle fires OnStatsUpdated + OnPlayerTurnStart immediately
        GameManager.Instance.StartCurrentBattle();
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance == null) return;
        BattleManager.Instance.OnStatsUpdated     -= OnStatsUpdated;
        BattleManager.Instance.OnMoveExecuted     -= OnMoveExecuted;
        BattleManager.Instance.OnPlayerTurnStart  -= OnPlayerTurnStart;
        BattleManager.Instance.OnMonsterTurnStart -= OnMonsterTurnStart;
        BattleManager.Instance.OnBattleEnd        -= OnBattleEnd;
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    private void OnStatsUpdated()
    {
        var hero    = BattleManager.Instance.Hero;
        var monster = BattleManager.Instance.Monster;

        heroHpBar.maxValue = hero.MaxHp;
        heroHpBar.value    = hero.CurrentHp;
        heroHpText.text    = $"{hero.CurrentHp} / {hero.MaxHp}";

        monsterHpBar.maxValue = monster.MaxHp;
        monsterHpBar.value    = monster.CurrentHp;
        monsterHpText.text    = $"{monster.CurrentHp} / {monster.MaxHp}";
        monsterNameText.text  = monster.Name;
    }

    private void OnMoveExecuted(string message)
    {
        StopAllCoroutines();
        feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, 1f);
        feedbackText.text  = message;
        StartCoroutine(FadeFeedback());
    }

    private void OnPlayerTurnStart()
    {
        var equipped = GameManager.Instance.EquippedMoves;
        for (int i = 0; i < moveButtons.Length; i++)
        {
            if (i < equipped.Count)
            {
                Move capturedMove = equipped[i];
                moveButtons[i].gameObject.SetActive(true);
                moveButtons[i].interactable = true;
                moveButtonTexts[i].text = capturedMove.name;
                moveButtons[i].onClick.RemoveAllListeners();
                moveButtons[i].onClick.AddListener(() => BattleManager.Instance.PlayerSelectMove(capturedMove));
            }
            else
            {
                moveButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnMonsterTurnStart()
    {
        foreach (var btn in moveButtons)
            btn.interactable = false;
    }

    private void OnBattleEnd(bool heroWon)
    {
        foreach (var btn in moveButtons)
            btn.interactable = false;

        if (heroWon) winPanel.SetActive(true);
        else         lossPanel.SetActive(true);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private IEnumerator FadeFeedback()
    {
        yield return new WaitForSeconds(1.5f);
        float elapsed = 0f;
        Color c = feedbackText.color;
        while (elapsed < 0.8f)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / 0.8f);
            feedbackText.color = c;
            yield return null;
        }
    }
}
