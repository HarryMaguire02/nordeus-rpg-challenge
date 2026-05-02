using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PostBattleUI : MonoBehaviour
{
    [Header("New Move")]
    [SerializeField] private TextMeshProUGUI newMoveNameText;
    [SerializeField] private TextMeshProUGUI newMoveTypeText;
    [SerializeField] private TextMeshProUGUI newMoveStatusText;
    [SerializeField] private Button equipButton;

    [Header("Equipped Slots (shown when full)")]
    [SerializeField] private GameObject equippedMovesPanel;
    [SerializeField] private Button[] equippedMoveSlotButtons;
    [SerializeField] private TextMeshProUGUI[] equippedMoveSlotTexts;

    [Header("Navigation")]
    [SerializeField] private Button proceedButton;

    private Move _newMove;

    private void Start()
    {
        _newMove = GameManager.Instance.LastLearnedMove;

        newMoveNameText.text = _newMove.name;
        newMoveTypeText.text = _newMove.type.ToString();

        equipButton.onClick.AddListener(OnEquipClicked);
        proceedButton.onClick.AddListener(() => GameManager.Instance.ProceedFromPostBattle());

        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        var gm = GameManager.Instance;
        bool alreadyEquipped = gm.EquippedMoves.Exists(m => m.id == _newMove.id);
        bool slotsFull       = gm.EquippedMoves.Count >= 4;

        if (alreadyEquipped)
        {
            newMoveStatusText.text   = "Already equipped!";
            equipButton.interactable = false;
            equippedMovesPanel.SetActive(false);
        }
        else if (!slotsFull)
        {
            newMoveStatusText.text   = "You have a free slot.";
            equipButton.interactable = true;
            equippedMovesPanel.SetActive(false);
        }
        else
        {
            newMoveStatusText.text   = "Slots full — unequip one to make room:";
            equipButton.interactable = false;
            equippedMovesPanel.SetActive(true);
            RefreshEquippedSlots();
        }
    }

    private void RefreshEquippedSlots()
    {
        var equipped = GameManager.Instance.EquippedMoves;
        for (int i = 0; i < equippedMoveSlotButtons.Length; i++)
        {
            if (i < equipped.Count)
            {
                equippedMoveSlotButtons[i].gameObject.SetActive(true);
                equippedMoveSlotTexts[i].text = equipped[i].name;

                Move capturedMove = equipped[i];
                equippedMoveSlotButtons[i].onClick.RemoveAllListeners();
                equippedMoveSlotButtons[i].onClick.AddListener(() =>
                {
                    GameManager.Instance.UnequipMove(capturedMove);
                    RefreshDisplay();
                });
            }
            else
            {
                equippedMoveSlotButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnEquipClicked()
    {
        GameManager.Instance.EquipMove(_newMove);
        RefreshDisplay();
    }
}
