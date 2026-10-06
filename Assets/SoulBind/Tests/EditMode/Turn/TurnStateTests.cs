using NUnit.Framework;
using SoulBind.Turn;

namespace SoulBind.Tests.Turn
{
    public sealed class TurnStateTests
    {
        #region Turn Tests

        [Test]
        public void BeginCreatesAnActiveTurnWithActionPoints()
        {
            var state = new TurnState();
            state.Begin(0, 3);
            Assert.That(state.IsActive, Is.True);
            Assert.That(state.ActionPoints, Is.EqualTo(3));
        }

        [Test]
        public void SpendingActionPointsRejectsUnaffordableActions()
        {
            var state = new TurnState();
            state.Begin(0, 2);
            Assert.That(state.TrySpend(3), Is.False);
            Assert.That(state.ActionPoints, Is.EqualTo(2));
        }

        [Test]
        public void EndingTurnAdvancesPlayerAndRound()
        {
            var state = new TurnState();
            state.Begin(0, 1);
            state.End();
            Assert.That(state.ActivePlayer, Is.EqualTo(1));
            Assert.That(state.Round, Is.EqualTo(2));
            Assert.That(state.IsActive, Is.False);
        }

        [Test]
        public void AddingActionPointsRestoresTacticalBudget()
        {
            var state = new TurnState();
            state.Begin(0, 1);
            state.AddActionPoints(2);
            Assert.That(state.ActionPoints, Is.EqualTo(3));
        }

        #endregion
    }
}
