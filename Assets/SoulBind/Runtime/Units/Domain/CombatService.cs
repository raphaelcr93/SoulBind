using SoulBind.Turn;

namespace SoulBind.Units
{
    public sealed class CombatService
    {
        #region Actions

        public bool TryAttack(UnitState attacker, UnitState target, TurnState turn, int actionCost = 1)
        {
            if (attacker == null || target == null || turn == null) return false;
            if (attacker.IsDefeated || target.IsDefeated || !turn.TrySpend(actionCost)) return false;
            target.ReceiveDamage(attacker.Attack);
            return true;
        }

        #endregion
    }
}
