using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PostBattleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moveNameText;
    [SerializeField] private TextMeshProUGUI moveTypeText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button proceedButton;

    [Header("Level Up")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI levelUpTitleText;
    [SerializeField] private Button[] levelUpOptionButtons;
    [SerializeField] private TextMeshProUGUI[] levelUpOptionLabels;

    private void Start()
    {
        var gm = GameManager.Instance;
        proceedButton.onClick.AddListener(() => gm.ProceedFromPostBattle());

        if (gm.PendingLevelUp)
            ShowLevelUpPanel(gm);
        else
        {
            levelUpPanel.SetActive(false);
            ShowMoveInfo(gm);
        }
    }

    private void ShowLevelUpPanel(GameManager gm)
    {
        levelUpPanel.SetActive(true);
        proceedButton.gameObject.SetActive(false);
        levelUpTitleText.text = $"Level {gm.HeroLevel} reached! Choose your path:";

        var options = gm.RunConfig.hero.levelUpOptions;
        for (int i = 0; i < levelUpOptionButtons.Length; i++)
        {
            int captured = i;
            var opt = options[i];
            levelUpOptionLabels[i].text =
                $"<b>{opt.label}</b>\n" +
                $"HP +{opt.stats.health}   ATK +{opt.stats.attack}   DEF +{opt.stats.defense}   MAG +{opt.stats.magic}";
            levelUpOptionButtons[i].onClick.RemoveAllListeners();
            levelUpOptionButtons[i].onClick.AddListener(() => OnOptionChosen(captured));
        }
    }

    private void OnOptionChosen(int index)
    {
        GameManager.Instance.ApplyLevelUpChoice(index);
        levelUpPanel.SetActive(false);
        proceedButton.gameObject.SetActive(true);
        ShowMoveInfo(GameManager.Instance);
    }

    private void ShowMoveInfo(GameManager gm)
    {
        var move = gm.LastLearnedMove;
        if (move == null) return;
        moveNameText.text = move.name;
        moveTypeText.text = move.type.ToString();
        statusText.text   = gm.LastMoveIsNew
            ? "New move added to your pool!"
            : "Unlucky! You already knew this move.";
    }
}
