using NordeusChallenge.Api.Models;

namespace NordeusChallenge.Api.Services;

public class MonsterMoveService
{
    private readonly Random _random = new();

    public Move PickMove(MonsterConfig monster, BattleState state)
    {
        var moves = monster.Moves;

        // When low HP, prefer sustain moves (heals, defensive buffs, magic buffs for drain life scaling)
        bool isLowHp = state.MonsterCurrentHp < state.MonsterMaxHp * 0.3;
        if (isLowHp)
        {
            var sustainMoves = moves.Where(m =>
                m.Primary.Kind == EffectKind.Heal ||
                m.Secondary?.Kind == EffectKind.Heal ||
                m.Primary.Kind == EffectKind.BuffSelfDefense ||
                m.Primary.Kind == EffectKind.BuffSelfMagic
            ).ToList();

            if (sustainMoves.Count > 0 && _random.NextDouble() < 0.6)
                return sustainMoves[_random.Next(sustainMoves.Count)];
        }

        return moves[_random.Next(moves.Count)];
    }
}
