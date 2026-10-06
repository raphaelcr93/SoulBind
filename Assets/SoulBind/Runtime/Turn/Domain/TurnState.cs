using System;

namespace SoulBind.Turn
{
    /// <summary>Small deterministic turn state used by local and future network simulations.</summary>
    public sealed class TurnState
    {
        #region Properties

        public int Round { get; private set; }
        public int ActivePlayer { get; private set; }
        public int ActionPoints { get; private set; }
        public bool IsActive { get; private set; }

        #endregion

        #region Turn Lifecycle

        public void Begin(int playerIndex, int actionPoints)
        {
            if (playerIndex < 0) throw new ArgumentOutOfRangeException(nameof(playerIndex));
            if (actionPoints < 1) throw new ArgumentOutOfRangeException(nameof(actionPoints));
            ActivePlayer = playerIndex;
            ActionPoints = actionPoints;
            Round = Math.Max(1, Round);
            IsActive = true;
        }

        public void End()
        {
            if (!IsActive) return;
            IsActive = false;
            ActivePlayer++;
            Round++;
        }

        #endregion

        #region Actions

        public bool CanAfford(int cost) => IsActive && cost > 0 && cost <= ActionPoints;

        public bool TrySpend(int cost)
        {
            if (!CanAfford(cost)) return false;
            ActionPoints -= cost;
            return true;
        }

        public void AddActionPoints(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            ActionPoints += amount;
        }

        #endregion
    }
}
