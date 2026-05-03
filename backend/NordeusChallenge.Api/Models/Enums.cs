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
