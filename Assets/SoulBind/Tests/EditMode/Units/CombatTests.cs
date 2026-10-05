using NUnit.Framework;
using SoulBind.Turn;
using SoulBind.Units;

namespace SoulBind.Tests.Units
{
    public sealed class CombatTests
    {
        #region Combat Tests

        [Test]
        public void AttackConsumesActionPointAndDealsDamage()
        {
            var attacker = new UnitState("a", "Attacker", 3, 10);
            var target = new UnitState("b", "Target", 1, 10);
            var turn = new TurnState();
            turn.Begin(0, 2);
            Assert.That(new CombatService().TryAttack(attacker, target, turn), Is.True);
            Assert.That(target.Health, Is.EqualTo(7));
            Assert.That(turn.ActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void DefeatedTargetCannotBeAttacked()
        {
            var attacker = new UnitState("a", "Attacker", 10, 10);
            var target = new UnitState("b", "Target", 1, 3);
            var turn = new TurnState();
            turn.Begin(0, 2);
            new CombatService().TryAttack(attacker, target, turn);
            Assert.That(target.IsDefeated, Is.True);
            Assert.That(new CombatService().TryAttack(attacker, target, turn), Is.False);
        }

        #endregion
    }
}
