using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Run config (fetched once from API) ───────────────────────────────────
    public RunConfig RunConfig { get; private set; }

    // ── Hero runtime state ───────────────────────────────────────────────────
    public int        HeroLevel        { get; private set; }
    public int        HeroXp           { get; private set; }
    public Stats      HeroCurrentStats { get; private set; }
    public List<Move> EquippedMoves    { get; private set; }
    public List<Move> LearnedMoves     { get; private set; }

    // ── Run state ────────────────────────────────────────────────────────────
    public int  MonstersDefeated   { get; private set; }
    public int  CurrentMonsterIndex { get; private set; }
    public Move LastLearnedMove     { get; private set; }
    public bool LastMoveIsNew       { get; private set; }
    public bool PendingLevelUp      { get; private set; }

    // ── Events ───────────────────────────────────────────────────────────────
    public event Action OnRunConfigLoaded;
    public event Action OnHeroStateChanged;
    public event Action<string> OnRunConfigLoadFailed;

    // ── Level-up tracking ────────────────────────────────────────────────────
    private List<Stats> _chosenLevelUpBoosts = new();

    // ── Save / load ──────────────────────────────────────────────────────────
    private const string SaveKey = "NordeusRpgSave";

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Run start ────────────────────────────────────────────────────────────

    public string SelectedHeroId { get; private set; }

    public void StartRun(string heroId)
    {
        DeleteSave();
        SelectedHeroId = heroId;
        ApiService.Instance.GetRunConfig(
            heroId,
            config =>
            {
                RunConfig = config;
                InitHeroState();
                OnRunConfigLoaded?.Invoke();
                SceneManager.LoadScene("Map");
            },
            error =>
            {
                Debug.LogError($"Failed to fetch run config: {error}");
                OnRunConfigLoadFailed?.Invoke(error);
            }
        );
    }

    private void InitHeroState()
    {
        HeroLevel        = 1;
        HeroXp           = 0;
        MonstersDefeated = 0;
        LastLearnedMove  = null;
        PendingLevelUp   = false;
        _chosenLevelUpBoosts = new List<Stats>();

        RecalculateStats();

        EquippedMoves = new List<Move>(RunConfig.hero.defaultMoves);
        LearnedMoves  = new List<Move>(RunConfig.hero.defaultMoves);
    }

    // ── Battle flow ──────────────────────────────────────────────────────────

    public void SelectEncounter(int monsterIndex)
    {
        CurrentMonsterIndex = monsterIndex;
        SceneManager.LoadScene("Battle");
    }

    public void StartCurrentBattle()
    {
        var monster = RunConfig.monsters[CurrentMonsterIndex];
        BattleManager.Instance.StartBattle(monster, HeroCurrentStats, EquippedMoves);
    }

    public void OnBattleWon()
    {
        AwardXp();
        LearnMove();
        if (!PendingLevelUp)
            OnHeroStateChanged?.Invoke();

        if (CurrentMonsterIndex >= MonstersDefeated)
            MonstersDefeated++;

        SceneManager.LoadScene("PostBattle");
    }

    public void RetryBattle()
    {
        SceneManager.LoadScene("Battle");
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene("Map");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ProceedFromPostBattle()
    {
        if (MonstersDefeated >= RunConfig.monsters.Count)
        {
            DeleteSave();
            SceneManager.LoadScene("MainMenu");
            return;
        }

        SceneManager.LoadScene("Map");
    }

    // ── Level-up choice ──────────────────────────────────────────────────────

    public void ApplyLevelUpChoice(int optionIndex)
    {
        _chosenLevelUpBoosts.Add(RunConfig.hero.levelUpOptions[optionIndex].stats);
        PendingLevelUp = false;
        RecalculateStats();
        OnHeroStateChanged?.Invoke();
    }

    // ── Move management ──────────────────────────────────────────────────────

    public void EquipMove(Move move)
    {
        if (EquippedMoves.Count >= 4 || EquippedMoves.Exists(m => m.id == move.id)) return;
        EquippedMoves.Add(move);
    }

    public void UnequipMove(Move move)
    {
        if (EquippedMoves.Count <= 1) return;
        EquippedMoves.RemoveAll(m => m.id == move.id);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void AwardXp()
    {
        HeroXp += RunConfig.hero.xpPerWin;
        if (HeroXp >= RunConfig.hero.xpToLevelUp)
        {
            HeroXp -= RunConfig.hero.xpToLevelUp;
            HeroLevel++;
            PendingLevelUp = true;
        }
    }

    private void LearnMove()
    {
        var monsterMoves = RunConfig.monsters[CurrentMonsterIndex].moves;
        var unlearnedMoves = monsterMoves.FindAll(m => !LearnedMoves.Exists(l => l.id == m.id));

        var pool = unlearnedMoves.Count > 0 ? unlearnedMoves : monsterMoves;
        LastLearnedMove = pool[UnityEngine.Random.Range(0, pool.Count)];
        LastMoveIsNew   = !LearnedMoves.Exists(m => m.id == LastLearnedMove.id);

        if (LastMoveIsNew)
            LearnedMoves.Add(LastLearnedMove);
    }

    private void RecalculateStats()
    {
        var b = RunConfig.hero.baseStats;
        int hp = b.health, atk = b.attack, def = b.defense, mag = b.magic;

        foreach (var boost in _chosenLevelUpBoosts)
        {
            hp += boost.health;
            atk += boost.attack;
            def += boost.defense;
            mag += boost.magic;
        }

        HeroCurrentStats = new Stats
        {
            health = hp,
            attack = atk,
            defense = def,
            magic = mag
        };
    }

    // ── Save / load ──────────────────────────────────────────────────────────

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public void SaveRun()
    {
        var equippedIds = new List<string>();
        foreach (var move in EquippedMoves)
            equippedIds.Add(move.id);

        var learnedIds = new List<string>();
        foreach (var move in LearnedMoves)
            learnedIds.Add(move.id);

        var save = new SaveData
        {
            heroId = SelectedHeroId,
            heroLevel = HeroLevel,
            heroXp = HeroXp,
            monstersDefeated = MonstersDefeated,
            chosenLevelUpBoosts = new List<Stats>(_chosenLevelUpBoosts),
            equippedMoveIds = equippedIds,
            learnedMoveIds = learnedIds
        };

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
        PlayerPrefs.Save();
    }

    public void SaveAndExit()
    {
        SaveRun();
        SceneManager.LoadScene("MainMenu");
    }

    public void ResumeRun()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            OnRunConfigLoadFailed?.Invoke("No save data found.");
            return;
        }

        var save = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
        ApiService.Instance.GetRunConfig(
            save.heroId,
            config =>
            {
                RunConfig = config;
                RestoreFromSave(save);
                OnRunConfigLoaded?.Invoke();
                SceneManager.LoadScene("Map");
            },
            error =>
            {
                Debug.LogError($"Failed to fetch run config on resume: {error}");
                OnRunConfigLoadFailed?.Invoke(error);
            }
        );
    }

    private void RestoreFromSave(SaveData save)
    {
        SelectedHeroId = save.heroId;
        HeroLevel = save.heroLevel;
        HeroXp = save.heroXp;
        MonstersDefeated = save.monstersDefeated;
        PendingLevelUp = false;
        LastLearnedMove = null;
        _chosenLevelUpBoosts = new List<Stats>(save.chosenLevelUpBoosts);
        RecalculateStats();

        var learnedMoves = new List<Move>();
        foreach (var id in save.learnedMoveIds)
        {
            var move = FindMoveById(id);
            if (move != null) learnedMoves.Add(move);
        }
        LearnedMoves = learnedMoves;

        var equippedMoves = new List<Move>();
        foreach (var id in save.equippedMoveIds)
        {
            var move = FindMoveById(id);
            if (move != null) equippedMoves.Add(move);
        }
        EquippedMoves = equippedMoves.Count > 0 ? equippedMoves : new List<Move>(RunConfig.hero.defaultMoves);
    }

    private Move FindMoveById(string id)
    {
        var heroMove = RunConfig.hero.defaultMoves.Find(m => m.id == id);
        if (heroMove != null) return heroMove;

        foreach (var monster in RunConfig.monsters)
        {
            var monsterMove = monster.moves.Find(m => m.id == id);
            if (monsterMove != null) return monsterMove;
        }
        return null;
    }

    private static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}
