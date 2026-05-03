using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    [Header("Sprites")]
    [SerializeField] private Image heroSprite;
    [SerializeField] private Image monsterSprite;
    [SerializeField] private Sprite[] heroSprites;
    [SerializeField] private Sprite[] monsterSprites;

    [Header("Battle Log")]
    [SerializeField] private ScrollRect battleLogScrollRect;
    [SerializeField] private Transform battleLogContent;

    [Header("Move Tooltip")]
    [SerializeField] private GameObject moveTooltipPanel;
    [SerializeField] private TextMeshProUGUI moveTooltipText;

    private Canvas _canvas;

    private void Start()
    {
        _canvas = GetComponentInParent<Canvas>();
        winPanel.SetActive(false);
        lossPanel.SetActive(false);
        feedbackText.text = string.Empty;
        if (moveTooltipPanel != null) moveTooltipPanel.SetActive(false);
        ClearBattleLog();
        SetupSprites();

        BattleManager.Instance.OnStatsUpdated    += OnStatsUpdated;
        BattleManager.Instance.OnMoveExecuted    += OnMoveExecuted;
        BattleManager.Instance.OnPlayerTurnStart += OnPlayerTurnStart;
        BattleManager.Instance.OnMonsterTurnStart += OnMonsterTurnStart;
        BattleManager.Instance.OnBattleEnd       += OnBattleEnd;

        continueButton.onClick.AddListener(() => GameManager.Instance.OnBattleWon());
        retryButton.onClick.AddListener(() => GameManager.Instance.RetryBattle());
        mapButton.onClick.AddListener(() => GameManager.Instance.ReturnToMap());

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
        AddLogEntry(message);
        StartCoroutine(FadeFeedback());
    }

    private void OnPlayerTurnStart()
    {
        if (moveTooltipPanel != null) moveTooltipPanel.SetActive(false);
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
                AddMoveTooltip(moveButtons[i], capturedMove);
            }
            else
            {
                moveButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnMonsterTurnStart()
    {
        if (moveTooltipPanel != null) moveTooltipPanel.SetActive(false);
        foreach (var btn in moveButtons)
            btn.interactable = false;
    }

    private void OnBattleEnd(bool heroWon)
    {
        if (moveTooltipPanel != null) moveTooltipPanel.SetActive(false);
        foreach (var btn in moveButtons)
            btn.interactable = false;

        if (heroWon) winPanel.SetActive(true);
        else         lossPanel.SetActive(true);
    }

    // ── Sprites ──────────────────────────────────────────────────────────────

    private static readonly string[] HeroOrder = { "knight", "rogue", "mage" };

    private void SetupSprites()
    {
        if (heroSprite != null && heroSprites != null)
        {
            int heroIdx = System.Array.IndexOf(HeroOrder, GameManager.Instance.SelectedHeroId);
            if (heroIdx >= 0 && heroIdx < heroSprites.Length)
                heroSprite.sprite = heroSprites[heroIdx];
        }

        if (monsterSprite != null && monsterSprites != null)
        {
            int idx = GameManager.Instance.CurrentMonsterIndex;
            if (idx >= 0 && idx < monsterSprites.Length)
                monsterSprite.sprite = monsterSprites[idx];
        }
    }

    // ── Battle Log ───────────────────────────────────────────────────────────

    private void ClearBattleLog()
    {
        if (battleLogContent == null) return;
        foreach (Transform child in battleLogContent)
            Destroy(child.gameObject);
    }

    private void AddLogEntry(string text)
    {
        if (battleLogContent == null || battleLogScrollRect == null) return;
        var go = new GameObject("LogEntry");
        go.transform.SetParent(battleLogContent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 13;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;
        go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        Canvas.ForceUpdateCanvases();
        battleLogScrollRect.verticalNormalizedPosition = 0f;
    }

    // ── Move Tooltip ─────────────────────────────────────────────────────────

    private void AddMoveTooltip(Button btn, Move move)
    {
        if (moveTooltipPanel == null) return;
        var trigger = btn.gameObject.GetComponent<EventTrigger>()
                      ?? btn.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var btnRect = btn.GetComponent<RectTransform>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => ShowTooltip(move, btnRect));
        trigger.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => moveTooltipPanel.SetActive(false));
        trigger.triggers.Add(exit);
    }

    private void ShowTooltip(Move move, RectTransform buttonRect)
    {
        if (moveTooltipPanel == null) return;
        moveTooltipPanel.SetActive(true);

        var tooltipRect = moveTooltipPanel.GetComponent<RectTransform>();
        var cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        var canvasRect = _canvas.GetComponent<RectTransform>();
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, buttonRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out Vector2 localPos);
        tooltipRect.anchoredPosition = localPos + new Vector2(0f, tooltipRect.rect.height * 0.5f + 12f);

        string typeLabel = move.type == MoveType.Physical ? "Physical" : "Magic";
        string desc = string.IsNullOrEmpty(move.description) ? "" : move.description;
        moveTooltipText.text = $"<b>{move.name}</b>  [{typeLabel}]\n{desc}";
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
