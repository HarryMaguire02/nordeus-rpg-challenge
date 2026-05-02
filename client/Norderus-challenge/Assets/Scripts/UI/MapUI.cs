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

    [Header("Move List")]
    [SerializeField] private Transform moveListContainer;

    private void Start()
    {
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
        RefreshMoveList();
    }

    // ── Encounters ───────────────────────────────────────────────────────────

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

            if (i < defeated)
            {
                encounterButtonTexts[i].text     = $"✓ {monsters[i].name}";
                encounterButtons[i].interactable = false;
            }
            else if (i == defeated)
            {
                encounterButtonTexts[i].text     = monsters[i].name;
                encounterButtons[i].interactable = true;
                int captured = i;
                encounterButtons[i].onClick.RemoveAllListeners();
                encounterButtons[i].onClick.AddListener(() =>
                    GameManager.Instance.SelectEncounter(captured));
            }
            else
            {
                encounterButtonTexts[i].text     = "???";
                encounterButtons[i].interactable = false;
            }
        }
    }

    // ── Hero stats ───────────────────────────────────────────────────────────

    private void RefreshHeroStats()
    {
        var gm      = GameManager.Instance;
        var s       = gm.HeroCurrentStats;
        int xpNeeded = gm.RunConfig.hero.xpToLevelUp;

        heroLevelText.text = $"Level {gm.HeroLevel}";
        heroXpText.text    = $"XP: {gm.HeroXp} / {xpNeeded}";
        heroStatsText.text = $"HP: {s.health}   ATK: {s.attack}   DEF: {s.defense}   MAG: {s.magic}";
    }

    // ── Move list ────────────────────────────────────────────────────────────

    private void RefreshMoveList()
    {
        foreach (Transform child in moveListContainer)
            Destroy(child.gameObject);

        var gm      = GameManager.Instance;
        var learned = gm.LearnedMoves;

        foreach (var move in learned)
        {
            bool equipped = gm.EquippedMoves.Exists(m => m.id == move.id);

            var go  = new GameObject(move.name);
            go.transform.SetParent(moveListContainer, worldPositionStays: false);

            var layout      = go.AddComponent<LayoutElement>();
            layout.minHeight = 40f;

            var img   = go.AddComponent<Image>();
            img.color = equipped
                ? new Color(0.2f, 0.8f, 0.2f, 1f)
                : new Color(0.7f, 0.7f, 0.7f, 1f);

            var btn          = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, worldPositionStays: false);

            var rt       = labelGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp       = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = equipped ? $"[E]  {move.name}  ({move.type})" : $"{move.name}  ({move.type})";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize  = 16f;
            tmp.color     = Color.black;

            Move capturedMove = move;
            btn.onClick.AddListener(() =>
            {
                if (gm.EquippedMoves.Exists(m => m.id == capturedMove.id))
                    gm.UnequipMove(capturedMove);
                else
                    gm.EquipMove(capturedMove);
                RefreshMoveList();
            });
        }
    }
}
