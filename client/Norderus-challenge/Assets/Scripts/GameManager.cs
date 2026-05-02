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
    public int  MonstersDefeated   { get; private set; }  // how far the player has progressed
    public int  CurrentMonsterIndex { get; private set; } // which monster is being fought right now
    public Move LastLearnedMove     { get; private set; }
    public bool LastMoveIsNew       { get; private set; }

    // ── Events ───────────────────────────────────────────────────────────────
    public event Action OnRunConfigLoaded;
    public event Action OnHeroStateChanged;
    public event Action<string> OnRunConfigLoadFailed;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Run start ────────────────────────────────────────────────────────────

    public void StartRun()
    {
        ApiService.Instance.GetRunConfig(
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
            SceneManager.LoadScene("MainMenu");
            return;
        }

        SceneManager.LoadScene("Map");
    }

    // ── Move management ──────────────────────────────────────────────────────

    public void EquipMove(Move move)
    {
        if (EquippedMoves.Count >= 4 || EquippedMoves.Exists(m => m.id == move.id)) return;
        EquippedMoves.Add(move);
    }

    public void UnequipMove(Move move)
    {
        if (EquippedMoves.Count <= 1) return; // must keep at least one move
        EquippedMoves.RemoveAll(m => m.id == move.id);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void AwardXp()
    {
        HeroXp += RunConfig.hero.xpPerWin;

        while (HeroXp >= RunConfig.hero.xpToLevelUp)
        {
            HeroXp -= RunConfig.hero.xpToLevelUp;
            HeroLevel++;
            RecalculateStats();
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
        var b   = RunConfig.hero.baseStats;
        var inc = RunConfig.hero.statIncreasePerLevel;
        int lvl = HeroLevel - 1;

        HeroCurrentStats = new Stats
        {
            health  = b.health  + lvl * inc.health,
            attack  = b.attack  + lvl * inc.attack,
            defense = b.defense + lvl * inc.defense,
            magic   = b.magic   + lvl * inc.magic
        };
    }
}
