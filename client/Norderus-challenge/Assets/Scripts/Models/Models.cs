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
    public string name;
    public Stats baseStats;
    public Stats statIncreasePerLevel;
    public int xpPerWin;
    public int xpToLevelUp;
    public List<Move> defaultMoves;
}

[Serializable]
public class RunConfig
{
    public HeroConfig hero;
    public List<MonsterConfig> monsters;
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

// Received from backend — the move the monster will play
[Serializable]
public class MonsterMoveResponse
{
    public Move move;
}
