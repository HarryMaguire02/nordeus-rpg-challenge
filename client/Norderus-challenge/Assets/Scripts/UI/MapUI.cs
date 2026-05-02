using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapUI : MonoBehaviour
{
    [Header("Encounters")]
    [SerializeField] private Button[] encounterButtons;
    [SerializeField] private TextMeshProUGUI[] encounterButtonTexts;

    [Header("Hero Stats")]
    [SerializeField] private TextMeshProUGUI heroLevelText;
    [SerializeField] private TextMeshProUGUI heroXpText;
    [SerializeField] private TextMeshProUGUI heroStatsText;

    [Header("Equipped Moves (right panel)")]
    [SerializeField] private Transform equippedMovesContainer;
    [SerializeField] private Button manageMovesButton;

    [Header("Move Management Panel")]
    [SerializeField] private GameObject moveManagementPanel;
    [SerializeField] private Transform equippedSlotsContainer;   // left side of panel
    [SerializeField] private Transform availableMovesContainer;  // right side of panel
    [SerializeField] private Button closePanelButton;

    private void Start()
    {
        moveManagementPanel.SetActive(false);
        manageMovesButton.onClick.AddListener(() => moveManagementPanel.SetActive(true));
        closePanelButton.onClick.AddListener(() => moveManagementPanel.SetActive(false));

        GameManager.Instance.OnHeroStateChanged += RefreshAll;
        RefreshAll();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnHeroStateChanged -= RefreshAll;
    }

    private void RefreshAll()
    {
        RefreshEncounters();
        RefreshHeroStats();
        RefreshEquippedMoves();
        RefreshManagementPanel();
    }

    // ── Encounters ───────────────────────────────────────────────────────────

    private static readonly Color ColorDefeated = new Color(0.20f, 0.70f, 0.20f, 1f);
    private static readonly Color ColorCurrent  = new Color(0.55f, 0.10f, 0.10f, 1f);
    private static readonly Color ColorFuture   = new Color(0.55f, 0.10f, 0.10f, 1f);

    private void RefreshEncounters()
    {
        var gm       = GameManager.Instance;
        var monsters = gm.RunConfig.monsters;
        int defeated = gm.MonstersDefeated;

        for (int i = 0; i < encounterButtons.Length; i++)
        {
            if (i >= monsters.Count)
            {
                encounterButtons[i].gameObject.SetActive(false);
                continue;
            }

            encounterButtons[i].gameObject.SetActive(true);

            int captured = i;
            encounterButtons[i].onClick.RemoveAllListeners();

            if (i < defeated)
            {
                encounterButtonTexts[i].text     = monsters[i].name;
                encounterButtons[i].interactable = true;
                encounterButtons[i].onClick.AddListener(() => GameManager.Instance.SelectEncounter(captured));
                SetButtonColor(encounterButtons[i], ColorDefeated);
            }
            else if (i == defeated)
            {
                encounterButtonTexts[i].text     = monsters[i].name;
                encounterButtons[i].interactable = true;
                encounterButtons[i].onClick.AddListener(() => GameManager.Instance.SelectEncounter(captured));
                SetButtonColor(encounterButtons[i], ColorCurrent);
            }
            else
            {
                encounterButtonTexts[i].text     = "???";
                encounterButtons[i].interactable = false;
                SetButtonColor(encounterButtons[i], ColorFuture);
            }
        }
    }

    private static void SetButtonColor(Button btn, Color normal)
    {
        var cb = btn.colors;
        cb.normalColor = normal;
        btn.colors = cb;
    }

    // ── Hero stats ───────────────────────────────────────────────────────────

    private void RefreshHeroStats()
    {
        var gm       = GameManager.Instance;
        var s        = gm.HeroCurrentStats;
        int xpNeeded = gm.RunConfig.hero.xpToLevelUp;

        heroLevelText.text = $"Level {gm.HeroLevel}";
        heroXpText.text    = $"XP: {gm.HeroXp} / {xpNeeded}";
        heroStatsText.text = $"HP: {s.health}   ATK: {s.attack}   DEF: {s.defense}   MAG: {s.magic}";
    }

    // ── Equipped moves (map right panel) ─────────────────────────────────────

    private void RefreshEquippedMoves()
    {
        foreach (Transform child in equippedMovesContainer)
            Destroy(child.gameObject);

        foreach (var move in GameManager.Instance.EquippedMoves)
            BuildMoveRow(equippedMovesContainer, move, equipped: true, clickable: false);
    }

    // ── Move management panel ────────────────────────────────────────────────

    private void RefreshManagementPanel()
    {
        foreach (Transform child in equippedSlotsContainer)
            Destroy(child.gameObject);

        foreach (Transform child in availableMovesContainer)
            Destroy(child.gameObject);

        var gm = GameManager.Instance;

        foreach (var move in gm.EquippedMoves)
        {
            var btn = BuildMoveRow(equippedSlotsContainer, move, equipped: true, clickable: true);
            Move captured = move;
            btn.onClick.AddListener(() =>
            {
                gm.UnequipMove(captured);
                RefreshAll();
            });
        }

        foreach (var move in gm.LearnedMoves)
        {
            if (gm.EquippedMoves.Exists(m => m.id == move.id)) continue;

            var btn = BuildMoveRow(availableMovesContainer, move, equipped: false, clickable: true);
            Move captured = move;
            btn.onClick.AddListener(() =>
            {
                gm.EquipMove(captured);
                RefreshAll();
            });
        }
    }

    // ── Shared row builder ───────────────────────────────────────────────────

    private Button BuildMoveRow(Transform parent, Move move, bool equipped, bool clickable)
    {
        var go = new GameObject(move.name);
        go.transform.SetParent(parent, worldPositionStays: false);

        var layout       = go.AddComponent<LayoutElement>();
        layout.minHeight = 45f;

        var img   = go.AddComponent<Image>();
        img.color = equipped ? new Color(0.20f, 0.80f, 0.20f, 1f)
                             : new Color(0.70f, 0.70f, 0.70f, 1f);

        var btn           = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.interactable  = clickable;

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, worldPositionStays: false);

        var rt       = labelGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp       = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = $"{move.name}  ({move.type})";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = 16f;
        tmp.color     = Color.black;

        return btn;
    }
}
