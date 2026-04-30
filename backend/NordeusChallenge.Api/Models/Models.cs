namespace NordeusChallenge.Api.Models;

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

// Duration > 0 means the effect lasts that many turns (buffs/debuffs)
public record MoveEffect(EffectKind Kind, int BaseValue, int Duration = 0);

// Damage formulas (applied client-side):
//   Physical damage = max(1, attacker.Attack + Primary.BaseValue - defender.Defense)
//   Magic damage    = max(1, attacker.Magic  + Primary.BaseValue)
//   Heal            = attacker.Magic + Primary.BaseValue
//   SelfDamage      = fixed HP cost = Primary.BaseValue (no stat scaling)
//   Buff/Debuff     = flat stat change of BaseValue for Duration turns
public record Move(string Id, string Name, MoveType Type, MoveEffect Primary, MoveEffect? Secondary = null);

public record Stats(int Health, int Attack, int Defense, int Magic);

public record MonsterConfig(string Id, string Name, Stats Stats, List<Move> Moves);

public record HeroConfig(
    string Name,
    Stats BaseStats,
    Stats StatIncreasePerLevel,
    int XpPerWin,
    int XpToLevelUp,
    List<Move> DefaultMoves
);

public record RunConfig(HeroConfig Hero, List<MonsterConfig> Monsters);

public record BattleState(
    string MonsterId,
    int MonsterCurrentHp,
    int MonsterMaxHp,
    int MonsterEffectiveAttack,
    int MonsterEffectiveDefense,
    int MonsterEffectiveMagic,
    int HeroCurrentHp,
    int HeroMaxHp,
    int HeroEffectiveAttack,
    int HeroEffectiveDefense,
    int HeroEffectiveMagic,
    int TurnNumber
);

public record MonsterMoveResponse(Move Move);
