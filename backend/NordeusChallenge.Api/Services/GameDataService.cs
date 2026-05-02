using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Services;

public static class GameDataService
{
    // ── Knight (hero default moveset) ────────────────────────────────────────
    private static readonly Move Slash       = new("slash",       "Slash",       MoveType.Physical, new(EffectKind.Damage,         5));
    private static readonly Move ShieldUp    = new("shield_up",   "Shield Up",   MoveType.Physical, new(EffectKind.BuffSelfDefense, 6, 2));
    private static readonly Move BattleCry   = new("battle_cry",  "Battle Cry",  MoveType.Physical, new(EffectKind.BuffSelfAttack,  6, 2));
    private static readonly Move SecondWind  = new("second_wind", "Second Wind", MoveType.Magic,    new(EffectKind.Heal,            8));

    // ── Witch ────────────────────────────────────────────────────────────────
    private static readonly Move ShadowBolt  = new("shadow_bolt", "Shadow Bolt", MoveType.Magic, new(EffectKind.Damage,            12));
    private static readonly Move DrainLife   = new("drain_life",  "Drain Life",  MoveType.Magic, new(EffectKind.Damage,             2), new(EffectKind.Heal, 2));
    private static readonly Move Curse       = new("curse",       "Curse",       MoveType.Magic, new(EffectKind.DebuffTargetAttack, 6, 2));
    private static readonly Move DarkPact    = new("dark_pact",   "Dark Pact",   MoveType.Magic, new(EffectKind.BuffSelfMagic,      8, 2), new(EffectKind.SelfDamage, 15));

    // ── Giant Spider ─────────────────────────────────────────────────────────
    private static readonly Move Bite        = new("bite",       "Bite",       MoveType.Physical, new(EffectKind.Damage,             5));
    private static readonly Move WebThrow    = new("web_throw",  "Web Throw",  MoveType.Physical, new(EffectKind.Damage,             2), new(EffectKind.DebuffTargetDefense, 5, 2));
    private static readonly Move Pounce      = new("pounce",     "Pounce",     MoveType.Physical, new(EffectKind.Damage,            12));
    private static readonly Move Skitter     = new("skitter",    "Skitter",    MoveType.Physical, new(EffectKind.BuffSelfDefense,    6, 2));

    // ── Dragon ───────────────────────────────────────────────────────────────
    private static readonly Move FlameBreath = new("flame_breath",  "Flame Breath",  MoveType.Magic,    new(EffectKind.Damage,            12));
    private static readonly Move ClawSwipe   = new("claw_swipe",    "Claw Swipe",    MoveType.Physical, new(EffectKind.Damage,             5));
    private static readonly Move Intimidate  = new("intimidate",    "Intimidate",    MoveType.Physical, new(EffectKind.DebuffTargetAttack, 8, 2));
    private static readonly Move DragonScales= new("dragon_scales", "Dragon Scales", MoveType.Physical, new(EffectKind.BuffSelfDefense,   10, 2));

    // ── Goblin Warrior ───────────────────────────────────────────────────────
    private static readonly Move RustyBlade  = new("rusty_blade", "Rusty Blade", MoveType.Physical, new(EffectKind.Damage,             5));
    private static readonly Move DirtyKick   = new("dirty_kick",  "Dirty Kick",  MoveType.Physical, new(EffectKind.Damage,             2), new(EffectKind.DebuffTargetDefense, 5, 2));
    private static readonly Move Frenzy      = new("frenzy",      "Frenzy",      MoveType.Physical, new(EffectKind.BuffSelfAttack,     6, 2));
    private static readonly Move Headbutt    = new("headbutt",    "Headbutt",    MoveType.Physical, new(EffectKind.Damage,            12));

    // ── Goblin Mage ──────────────────────────────────────────────────────────
    private static readonly Move Firebolt    = new("firebolt",    "Firebolt",    MoveType.Magic, new(EffectKind.Damage,              5));
    private static readonly Move ArcaneSurge = new("arcane_surge","Arcane Surge",MoveType.Magic, new(EffectKind.BuffSelfMagic,       6, 2));
    private static readonly Move ManaDrain   = new("mana_drain",  "Mana Drain",  MoveType.Magic, new(EffectKind.Damage,              2), new(EffectKind.DebuffTargetMagic, 6, 2));
    private static readonly Move HexShield   = new("hex_shield",  "Hex Shield",  MoveType.Magic, new(EffectKind.BuffSelfDefense,     6, 2));

    // ─────────────────────────────────────────────────────────────────────────

    public static RunConfig GetRunConfig() => new(
        Hero: new HeroConfig(
            Name: "Knight",
            BaseStats:             new Stats(Health: 10, Attack: 15, Defense: 10, Magic: 10),
            StatIncreasePerLevel:  new Stats(Health:  15, Attack:  5, Defense:  5, Magic:  5),
            XpPerWin:    50,
            XpToLevelUp: 100,
            DefaultMoves: [Slash, ShieldUp, BattleCry, SecondWind]
        ),
        Monsters:
        [
            new("goblin_warrior", "Goblin Warrior", new Stats(60,  16,  5,  3), [RustyBlade, DirtyKick,  Frenzy,      Headbutt    ]),
            new("giant_spider",   "Giant Spider",   new Stats(75,  18,  8,  3), [Bite,       WebThrow,   Pounce,      Skitter     ]),
            new("goblin_mage",    "Goblin Mage",    new Stats(65,   5,  5, 15), [Firebolt,   ArcaneSurge,ManaDrain,   HexShield   ]),
            new("witch",          "Witch",          new Stats(70,   5,  8, 18), [ShadowBolt, DrainLife,  Curse,       DarkPact    ]),
            new("dragon",         "Dragon",         new Stats(120, 24, 15, 20), [FlameBreath,ClawSwipe,  Intimidate,  DragonScales]),
        ]
    );

    public static MonsterConfig? GetMonster(string id) =>
        GetRunConfig().Monsters.FirstOrDefault(m => m.Id == id);
}
