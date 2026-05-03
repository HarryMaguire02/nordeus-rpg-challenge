using System;
using System.Collections.Generic;

// Must match backend enum order exactly — JsonUtility serializes enums as integers
public enum MoveType { Physical, Magic }

public enum EffectKind
{
    Damage,
    Heal,
    SelfDamage,
    BuffSelfAttack,
    BuffSelfDefense,
    BuffSelfMagic,
    DebuffTargetAttack,
    DebuffTargetDefense,
    DebuffTargetMagic
}

[Serializable]
public class MoveEffect
{
    public EffectKind kind;
    public int baseValue;
    public int duration;
}

[Serializable]
public class Move
{
    public string id;
    public string name;
    public MoveType type;
    public string description;
    public MoveEffect primary;
    public MoveEffect secondary; // null if move has no secondary effect
}

[Serializable]
public class Stats
{
    public int health;
    public int attack;
    public int defense;
    public int magic;
}

[Serializable]
public class LevelUpOption
{
    public string label;
    public Stats stats;
}

[Serializable]
public class MonsterConfig
{
    public string id;
    public string name;
    public Stats stats;
    public List<Move> moves;
}

[Serializable]
public class HeroConfig
{
    public string id;
    public string name;
    public string description;
    public Stats baseStats;
    public List<LevelUpOption> levelUpOptions;
    public int xpPerWin;
    public int xpToLevelUp;
    public List<Move> defaultMoves;
}

[Serializable]
public class HeroSummary
{
    public string id;
    public string name;
    public string description;
    public Stats baseStats;
}

[Serializable]
public class HeroListResponse
{
    public List<HeroSummary> heroes;
}

[Serializable]
public class RunConfig
{
    public HeroConfig hero;
    public List<MonsterConfig> monsters;
}

// Persisted to PlayerPrefs when the player saves mid-run
[Serializable]
public class SaveData
{
    public string heroId;
    public int heroLevel;
    public int heroXp;
    public int monstersDefeated;
    public List<Stats> chosenLevelUpBoosts;
    public List<string> equippedMoveIds;
    public List<string> learnedMoveIds;
}

// Sent from Unity to backend each turn
[Serializable]
public class BattleStateRequest
{
    public string monsterId;
    public int monsterCurrentHp;
    public int monsterMaxHp;
    public int monsterEffectiveAttack;
    public int monsterEffectiveDefense;
    public int monsterEffectiveMagic;
    public int heroCurrentHp;
    public int heroMaxHp;
    public int heroEffectiveAttack;
    public int heroEffectiveDefense;
    public int heroEffectiveMagic;
    public int turnNumber;
}

