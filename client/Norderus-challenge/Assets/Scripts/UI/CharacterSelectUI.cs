using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Transform heroCardsPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button backButton;
    [SerializeField] private Sprite[] heroSprites;

    private readonly List<Button> _selectButtons = new();

    private void Start()
    {
        statusText.text = "Loading heroes...";

        if (backButton != null)
            backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        GameManager.Instance.OnRunConfigLoadFailed += OnRunConfigLoadFailed;
        ApiService.Instance.GetHeroes(OnHeroesLoaded, OnLoadError);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRunConfigLoadFailed -= OnRunConfigLoadFailed;
    }

    private void OnHeroesLoaded(HeroListResponse response)
    {
        statusText.text = string.Empty;
        for (int i = 0; i < response.heroes.Count; i++)
            BuildHeroCard(response.heroes[i], i);
    }

    private void BuildHeroCard(HeroSummary hero, int index)
    {
        var card = new GameObject(hero.name, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(heroCardsPanel, false);
        card.GetComponent<Image>().color = new Color(30f / 255f, 25f / 255f, 20f / 255f);

        var vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 20, 20);
        vlg.spacing = 12;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        card.AddComponent<LayoutElement>().flexibleWidth = 1;

        if (heroSprites != null && index < heroSprites.Length && heroSprites[index] != null)
        {
            var spriteGo = new GameObject("HeroSprite", typeof(RectTransform), typeof(Image));
            spriteGo.transform.SetParent(card.transform, false);
            var img = spriteGo.GetComponent<Image>();
            img.sprite = heroSprites[index];
            img.preserveAspect = true;
            spriteGo.AddComponent<LayoutElement>().preferredHeight = 160;
        }

        AddLabel(card.transform, hero.name, 32, new Color(1f, 215f / 255f, 0f), 45);

        var descTmp = AddLabel(card.transform, hero.description, 16, Color.white, 70).GetComponent<TextMeshProUGUI>();
        descTmp.enableWordWrapping = true;

        var s = hero.baseStats;
        AddLabel(card.transform,
            $"HP {s.health}    ATK {s.attack}    DEF {s.defense}    MAG {s.magic}",
            15, new Color(0.75f, 0.75f, 0.75f), 28);

        BuildSelectButton(card.transform, hero.id);
    }

    private void BuildSelectButton(Transform parent, string heroId)
    {
        var btnGo = new GameObject("SelectButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(parent, false);
        btnGo.GetComponent<Image>().color = new Color(140f / 255f, 25f / 255f, 25f / 255f);
        btnGo.AddComponent<LayoutElement>().preferredHeight = 52;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(btnGo.transform, false);
        var rt = labelGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "Select";
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var btn = btnGo.GetComponent<Button>();
        btn.onClick.AddListener(() => OnHeroSelected(heroId));
        _selectButtons.Add(btn);
    }

    private static GameObject AddLabel(Transform parent, string text, float fontSize, Color color, float height)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().preferredHeight = height;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    private void OnHeroSelected(string heroId)
    {
        foreach (var btn in _selectButtons)
            btn.interactable = false;
        statusText.text = "Starting run...";
        GameManager.Instance.StartRun(heroId);
    }

    private void OnLoadError(string error)
    {
        statusText.text = $"Error: {error}\nMake sure the server is running.";
    }

    private void OnRunConfigLoadFailed(string error)
    {
        foreach (var btn in _selectButtons)
            btn.interactable = true;
        statusText.text = $"Error: {error}";
    }
}
