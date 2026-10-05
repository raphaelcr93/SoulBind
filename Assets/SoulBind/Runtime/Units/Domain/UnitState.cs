using System;

namespace SoulBind.Units
{
    public sealed class UnitState
    {
        #region Properties

        public string Id { get; }
        public string Name { get; }
        public int Attack { get; }
        public int MaxHealth { get; }
        public int Health { get; private set; }
        public bool IsDefeated => Health <= 0;

        #endregion

        #region Construction

        public UnitState(string id, string name, int attack, int maxHealth)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Unit id is required.", nameof(id));
            if (attack < 1) throw new ArgumentOutOfRangeException(nameof(attack));
            if (maxHealth < 1) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            Id = id;
            Name = string.IsNullOrWhiteSpace(name) ? id : name;
            Attack = attack;
            MaxHealth = maxHealth;
            Health = maxHealth;
        }

        #endregion

        #region Combat State

        public int ReceiveDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            var applied = Math.Min(amount, Health);
            Health -= applied;
            return applied;
        }

        #endregion
    }
}
