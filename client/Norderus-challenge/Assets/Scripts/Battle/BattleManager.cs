using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // ── Runtime character ────────────────────────────────────────────────────

    public class StatModifier
    {
        public EffectKind Kind;
        public int Value;
        public int TurnsRemaining;
    }

    public class BattleCharacter
    {
        public string Name;
        public int CurrentHp;
        public int MaxHp;
        public Stats BaseStats;
        public List<Move> Moves;
        public readonly List<StatModifier> Modifiers = new();

        public int EffectiveAttack => Mathf.Max(1, BaseStats.attack + ModSum(EffectKind.BuffSelfAttack,   EffectKind.DebuffTargetAttack));
        public int EffectiveDefense => Mathf.Max(0, BaseStats.defense + ModSum(EffectKind.BuffSelfDefense,  EffectKind.DebuffTargetDefense));
        public int EffectiveMagic => Mathf.Max(1, BaseStats.magic + ModSum(EffectKind.BuffSelfMagic,    EffectKind.DebuffTargetMagic));

        private int ModSum(EffectKind buff, EffectKind debuff) =>
            Modifiers.Where(m => m.Kind == buff).Sum(m => m.Value) -
            Modifiers.Where(m => m.Kind == debuff).Sum(m => m.Value);

        public void TickModifiers() =>
            Modifiers.RemoveAll(m => --m.TurnsRemaining <= 0);
    }

    // ── Events ───────────────────────────────────────────────────────────────

    public event Action OnPlayerTurnStart;
    public event Action OnMonsterTurnStart;
    public event Action<string> OnMoveExecuted;
    public event Action OnStatsUpdated;
    public event Action<bool> OnBattleEnd;

    // ── State ────────────────────────────────────────────────────────────────

    public BattleCharacter Hero { get; private set; }
    public BattleCharacter Monster { get; private set; }
    public int TurnNumber { get; private set; }

    private MonsterConfig _monsterConfig;
    private bool _battleOver;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public void StartBattle(MonsterConfig monster, Stats heroStats, List<Move> heroMoves)
    {
        _monsterConfig = monster;
        _battleOver = false;
        TurnNumber = 0;

        Hero = new BattleCharacter
        {
            Name = "Knight",
            MaxHp = heroStats.health,
            CurrentHp = heroStats.health,
            BaseStats = heroStats,
            Moves = heroMoves
        };

        Monster = new BattleCharacter
        {
            Name = monster.name,
            MaxHp = monster.stats.health,
            CurrentHp = monster.stats.health,
            BaseStats = monster.stats,
            Moves = monster.moves
        };

        OnStatsUpdated?.Invoke();
        OnPlayerTurnStart?.Invoke();
    }

    public void PlayerSelectMove(Move move)
    {
        if (_battleOver) return;
        StartCoroutine(ExecuteTurn(move));
    }

    // ── Turn loop ────────────────────────────────────────────────────────────

    private IEnumerator ExecuteTurn(Move move)
    {
        TurnNumber++;
        OnMonsterTurnStart?.Invoke(); // lock player input while processing

        ApplyMove(move, Hero, Monster);
        OnMoveExecuted?.Invoke($"Knight used {move.name}!");
        OnStatsUpdated?.Invoke();

        if (CheckBattleEnd()) yield break;
        yield return new WaitForSeconds(1f);

        bool waiting = true;
        ApiService.Instance.GetMonsterMove(BuildBattleState(),
            move =>
            {
                ApplyMove(move, Monster, Hero);
                OnMoveExecuted?.Invoke($"{Monster.Name} used {move.name}!");
                OnStatsUpdated?.Invoke();
                waiting = false;
            },
            error =>
            {
                Debug.LogError($"Monster move API error: {error}");
                waiting = false;
            });

        yield return new WaitUntil(() => !waiting);

        if (CheckBattleEnd()) yield break;

        Hero.TickModifiers();
        Monster.TickModifiers();
        OnStatsUpdated?.Invoke();

        yield return new WaitForSeconds(0.5f);
        OnPlayerTurnStart?.Invoke();
    }

    // ── Move application ─────────────────────────────────────────────────────

    private void ApplyMove(Move move, BattleCharacter attacker, BattleCharacter defender)
    {
        ApplyEffect(move.primary, move.type, attacker, defender);
        if (move.secondary != null && move.secondary.IsValid)
            ApplyEffect(move.secondary, move.type, attacker, defender);
    }

    private void ApplyEffect(MoveEffect effect, MoveType moveType, BattleCharacter attacker, BattleCharacter defender)
    {
        switch (effect.kind)
        {
            case EffectKind.Damage:
                int dmg = moveType == MoveType.Physical
                    ? Mathf.Max(1, attacker.EffectiveAttack + effect.baseValue - defender.EffectiveDefense)
                    : Mathf.Max(1, attacker.EffectiveMagic  + effect.baseValue);
                defender.CurrentHp = Mathf.Max(0, defender.CurrentHp - dmg);
                break;

            case EffectKind.Heal:
                int heal = attacker.EffectiveMagic + effect.baseValue;
                attacker.CurrentHp = Mathf.Min(attacker.MaxHp, attacker.CurrentHp + heal);
                break;

            case EffectKind.SelfDamage:
                attacker.CurrentHp = Mathf.Max(0, attacker.CurrentHp - effect.baseValue);
                break;

            case EffectKind.BuffSelfAttack:
            case EffectKind.BuffSelfDefense:
            case EffectKind.BuffSelfMagic:
                attacker.Modifiers.Add(new StatModifier { Kind = effect.kind, Value = effect.baseValue, TurnsRemaining = effect.duration });
                break;

            case EffectKind.DebuffTargetAttack:
            case EffectKind.DebuffTargetDefense:
            case EffectKind.DebuffTargetMagic:
                defender.Modifiers.Add(new StatModifier { Kind = effect.kind, Value = effect.baseValue, TurnsRemaining = effect.duration });
                break;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private bool CheckBattleEnd()
    {
        if (Monster.CurrentHp <= 0) { _battleOver = true; OnBattleEnd?.Invoke(true);  return true; }
        if (Hero.CurrentHp <= 0) { _battleOver = true; OnBattleEnd?.Invoke(false); return true; }
        return false;
    }

    private BattleStateRequest BuildBattleState() => new()
    {
        monsterId = _monsterConfig.id,
        monsterCurrentHp = Monster.CurrentHp,
        monsterMaxHp = Monster.MaxHp,
        monsterEffectiveAttack = Monster.EffectiveAttack,
        monsterEffectiveDefense = Monster.EffectiveDefense,
        monsterEffectiveMagic = Monster.EffectiveMagic,
        heroCurrentHp = Hero.CurrentHp,
        heroMaxHp = Hero.MaxHp,
        heroEffectiveAttack = Hero.EffectiveAttack,
        heroEffectiveDefense = Hero.EffectiveDefense,
        heroEffectiveMagic = Hero.EffectiveMagic,
        turnNumber = TurnNumber
    };
}
