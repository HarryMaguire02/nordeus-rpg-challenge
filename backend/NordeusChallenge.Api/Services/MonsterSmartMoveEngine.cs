using NordeusChallenge.Api.Dtos;
using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Services;

public class MonsterSmartMoveEngine : IMonsterMoveEngine
{
    public MoveDto PickMove(MonsterConfigDto monster, BattleStateDto state)
    {
        var moves = monster.Moves;

        var damageMoves = moves
            .Where(m => HasEffect(m, EffectKind.Damage))
            .OrderByDescending(GetHighestDamageValue)
            .ToList();

        var heavyDamageMoves = damageMoves.Where(m => GetHighestDamageValue(m) >= 10).ToList();

        var sustainMoves = moves.Where(m => HasEffect(m,
            EffectKind.Heal, EffectKind.BuffSelfDefense, EffectKind.BuffSelfMagic)).ToList();

        var debuffOrDefMoves = moves.Where(m => HasEffect(m,
            EffectKind.DebuffTargetAttack, EffectKind.DebuffTargetDefense,
            EffectKind.BuffSelfDefense)).ToList();

        // 1. Hero near death → go for the kill
        if (state.HeroCurrentHp < state.HeroMaxHp * 0.25)
        {
            var killMoves = heavyDamageMoves.Count > 0 ? heavyDamageMoves : damageMoves;
            if (killMoves.Count > 0 && Random.Shared.NextDouble() < 0.85)
                return PickRandom(killMoves);
        }

        // 2. Monster near death → survive
        if (state.MonsterCurrentHp < state.MonsterMaxHp * 0.25
            && sustainMoves.Count > 0 && Random.Shared.NextDouble() < 0.75)
        {
            return PickRandom(sustainMoves);
        }

        // 3. Monster has an offensive buff active → capitalize with heavy damage
        bool attackBuffed = state.MonsterEffectiveAttack > monster.Stats.Attack;
        bool magicBuffed = state.MonsterEffectiveMagic  > monster.Stats.Magic;
        if ((attackBuffed || magicBuffed) && Random.Shared.NextDouble() < 0.80)
        {
            var capMoves = heavyDamageMoves.Count > 0 ? heavyDamageMoves : damageMoves;
            if (capMoves.Count > 0)
                return PickRandom(capMoves);
        }

        // 4. Hero is threatening → debuff or build defense
        if (state.HeroEffectiveAttack > state.MonsterEffectiveDefense * 1.5
            && debuffOrDefMoves.Count > 0 && Random.Shared.NextDouble() < 0.65)
        {
            return PickRandom(debuffOrDefMoves);
        }

        // 5. Fallback low-HP sustain bias
        if (state.MonsterCurrentHp < state.MonsterMaxHp * 0.30
            && sustainMoves.Count > 0 && Random.Shared.NextDouble() < 0.55)
        {
            return PickRandom(sustainMoves);
        }

        return PickRandom(moves);
    }

    private static MoveDto PickRandom(List<MoveDto> moves) =>
        moves[Random.Shared.Next(moves.Count)];

    private static bool HasEffect(MoveDto move, params EffectKind[] kinds)
    {
        return kinds.Contains(move.Primary.Kind) ||
               (move.Secondary != null && kinds.Contains(move.Secondary.Kind));
    }

    private static int GetHighestDamageValue(MoveDto move)
    {
        int dmg = 0;
        if (move.Primary.Kind == EffectKind.Damage)
        {
            dmg += move.Primary.BaseValue;
        }
        if (move.Secondary != null && move.Secondary.Kind == EffectKind.Damage)
        {
            dmg += move.Secondary.BaseValue;
        }
        return dmg;
    }
}
