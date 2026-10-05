using NUnit.Framework;
using SoulBind.Cards;
using SoulBind.Turn;

namespace SoulBind.Tests.Cards
{
    public sealed class CardTests
    {
        #region Card Tests

        [Test]
        public void PlayingCardConsumesCostAndRemovesItFromHand()
        {
            var hand = new CardHand();
            hand.Add(new CardState("strike", "Strike", 1));
            var turn = new TurnState();
            turn.Begin(0, 2);
            Assert.That(hand.TryPlay(0, turn), Is.True);
            Assert.That(hand.Cards, Is.Empty);
            Assert.That(turn.ActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void CardRemainsInHandWhenCostCannotBePaid()
        {
            var hand = new CardHand();
            hand.Add(new CardState("heavy", "Heavy Strike", 3));
            var turn = new TurnState();
            turn.Begin(0, 1);
            Assert.That(hand.TryPlay(0, turn), Is.False);
            Assert.That(hand.Cards, Has.Count.EqualTo(1));
        }

        #endregion
    }
}
