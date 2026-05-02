using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Services;

public static class GameDataService
{
    // ── Knight (hero default moveset) ────────────────────────────────────────
    private static readonly Move Slash       = new("slash",       "Slash",       MoveType.Physical, "Strike with your sword, dealing moderate physical damage.",                              new(EffectKind.Damage,         5));
    private static readonly Move ShieldUp    = new("shield_up",   "Shield Up",   MoveType.Physical, "Raise your shield, boosting Defense for 2 turns.",                                      new(EffectKind.BuffSelfDefense, 6, 2));
    private static readonly Move BattleCry   = new("battle_cry",  "Battle Cry",  MoveType.Physical, "Bellow a war cry, boosting Attack for 2 turns.",                                        new(EffectKind.BuffSelfAttack,  6, 2));
    private static readonly Move SecondWind  = new("second_wind", "Second Wind", MoveType.Magic,    "Draw on inner reserves, restoring HP based on Magic stat.",                             new(EffectKind.Heal,            8));

    // ── Witch ────────────────────────────────────────────────────────────────
    private static readonly Move ShadowBolt  = new("shadow_bolt", "Shadow Bolt", MoveType.Magic,    "Hurl a bolt of dark energy dealing heavy magic damage.",                                new(EffectKind.Damage,            12));
    private static readonly Move DrainLife   = new("drain_life",  "Drain Life",  MoveType.Magic,    "Leech the target's life force, dealing damage and healing self.",                       new(EffectKind.Damage,             2), new(EffectKind.Heal, 2));
    private static readonly Move Curse       = new("curse",       "Curse",       MoveType.Magic,    "Afflict the target with a curse, reducing their Attack for 2 turns.",                  new(EffectKind.DebuffTargetAttack, 6, 2));
    private static readonly Move DarkPact    = new("dark_pact",   "Dark Pact",   MoveType.Magic,    "Sacrifice 15 HP to greatly empower Magic for 2 turns.",                                 new(EffectKind.BuffSelfMagic,      8, 2), new(EffectKind.SelfDamage, 15));

    // ── Giant Spider ─────────────────────────────────────────────────────────
    private static readonly Move Bite        = new("bite",       "Bite",       MoveType.Physical, "Sink venomous fangs in for moderate physical damage.",                                    new(EffectKind.Damage,             5));
    private static readonly Move WebThrow    = new("web_throw",  "Web Throw",  MoveType.Physical, "Hurl sticky webbing, dealing light damage and reducing target Defense for 2 turns.",     new(EffectKind.Damage,             2), new(EffectKind.DebuffTargetDefense, 5, 2));
    private static readonly Move Pounce      = new("pounce",     "Pounce",     MoveType.Physical, "Leap onto the target, dealing heavy physical damage.",                                   new(EffectKind.Damage,            12));
    private static readonly Move Skitter     = new("skitter",    "Skitter",    MoveType.Physical, "Dodge and weave erratically, boosting own Defense for 2 turns.",                        new(EffectKind.BuffSelfDefense,    6, 2));

    // ── Dragon ───────────────────────────────────────────────────────────────
    private static readonly Move FlameBreath = new("flame_breath",  "Flame Breath",  MoveType.Magic,    "Exhale a torrent of fire dealing heavy magic damage.",                             new(EffectKind.Damage,            12));
    private static readonly Move ClawSwipe   = new("claw_swipe",    "Claw Swipe",    MoveType.Physical, "Rake with massive claws for moderate physical damage.",                            new(EffectKind.Damage,             5));
    private static readonly Move Intimidate  = new("intimidate",    "Intimidate",    MoveType.Physical, "Roar ferociously, lowering target Attack for 2 turns.",                           new(EffectKind.DebuffTargetAttack, 8, 2));
    private static readonly Move DragonScales= new("dragon_scales", "Dragon Scales", MoveType.Physical, "Harden scales to greatly boost Defense for 2 turns.",                             new(EffectKind.BuffSelfDefense,   10, 2));

    // ── Goblin Warrior ───────────────────────────────────────────────────────
    private static readonly Move RustyBlade  = new("rusty_blade", "Rusty Blade", MoveType.Physical, "Hack with a corroded blade for moderate physical damage.",                             new(EffectKind.Damage,             5));
    private static readonly Move DirtyKick   = new("dirty_kick",  "Dirty Kick",  MoveType.Physical, "A cheap shin kick dealing light damage and reducing target Defense for 2 turns.",     new(EffectKind.Damage,             2), new(EffectKind.DebuffTargetDefense, 5, 2));
    private static readonly Move Frenzy      = new("frenzy",      "Frenzy",      MoveType.Physical, "Work into a combat frenzy, boosting own Attack for 2 turns.",                        new(EffectKind.BuffSelfAttack,     6, 2));
    private static readonly Move Headbutt    = new("headbutt",    "Headbutt",    MoveType.Physical, "A reckless headbutt delivering heavy physical damage.",                               new(EffectKind.Damage,            12));

    // ── Goblin Mage ──────────────────────────────────────────────────────────
    private static readonly Move Firebolt    = new("firebolt",    "Firebolt",    MoveType.Magic,    "Launch a small fireball dealing moderate magic damage.",                               new(EffectKind.Damage,              5));
    private static readonly Move ArcaneSurge = new("arcane_surge","Arcane Surge",MoveType.Magic,    "Channel raw arcane power, boosting own Magic for 2 turns.",                           new(EffectKind.BuffSelfMagic,       6, 2));
    private static readonly Move ManaDrain   = new("mana_drain",  "Mana Drain",  MoveType.Magic,    "Siphon magical energy, dealing light damage and reducing target Magic for 2 turns.",  new(EffectKind.Damage,              2), new(EffectKind.DebuffTargetMagic, 6, 2));
    private static readonly Move HexShield   = new("hex_shield",  "Hex Shield",  MoveType.Magic,    "Weave a magical barrier, boosting own Defense for 2 turns.",                          new(EffectKind.BuffSelfDefense,     6, 2));

    // ─────────────────────────────────────────────────────────────────────────

    public static RunConfig GetRunConfig() => new(
        Hero: new HeroConfig(
            Name: "Knight",
            BaseStats:             new Stats(Health: 100, Attack: 15, Defense: 10, Magic: 10),
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
