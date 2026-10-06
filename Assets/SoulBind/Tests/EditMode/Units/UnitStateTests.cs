using NUnit.Framework;
using SoulBind.Units;

namespace SoulBind.Tests.Units
{
    public sealed class UnitStateTests
    {
        [Test]
        public void RestoreHealthDoesNotExceedMaximum()
        {
            var unit = new UnitState("player", "Player", 3, 10);
            unit.ReceiveDamage(4);
            Assert.That(unit.RestoreHealth(20), Is.EqualTo(4));
            Assert.That(unit.Health, Is.EqualTo(10));
        }

        [Test]
        public void RestoreHealthReturnsZeroAtFullHealth()
        {
            var unit = new UnitState("player", "Player", 3, 10);
            Assert.That(unit.RestoreHealth(3), Is.EqualTo(0));
            Assert.That(unit.Health, Is.EqualTo(10));
        }
    }
}
