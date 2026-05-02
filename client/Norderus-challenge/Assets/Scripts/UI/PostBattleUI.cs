using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PostBattleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moveNameText;
    [SerializeField] private TextMeshProUGUI moveTypeText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button proceedButton;

    private void Start()
    {
        var gm   = GameManager.Instance;
        var move = gm.LastLearnedMove;

        moveNameText.text = move.name;
        moveTypeText.text = move.type.ToString();
        statusText.text   = gm.LastMoveIsNew
            ? "New move added to your pool!"
            : "Unlucky! You already knew this move.";

        proceedButton.onClick.AddListener(() => gm.ProceedFromPostBattle());
    }
}
