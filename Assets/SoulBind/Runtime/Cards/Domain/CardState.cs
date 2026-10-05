using System;
using System.Collections.Generic;
using SoulBind.Turn;

namespace SoulBind.Cards
{
    public sealed class CardState
    {
        #region Properties

        public string Id { get; }
        public string Name { get; }
        public int ActionCost { get; }

        #endregion

        #region Construction

        public CardState(string id, string name, int actionCost)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Card id is required.", nameof(id));
            if (actionCost < 1) throw new ArgumentOutOfRangeException(nameof(actionCost));
            Id = id;
            Name = string.IsNullOrWhiteSpace(name) ? id : name;
            ActionCost = actionCost;
        }

        #endregion
    }

    public sealed class CardHand
    {
        #region State

        private readonly List<CardState> cards = new List<CardState>();
        public IReadOnlyList<CardState> Cards => cards;

        #endregion

        #region Hand Operations

        public void Add(CardState card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            cards.Add(card);
        }

        public bool TryPlay(int index, TurnState turn)
        {
            if (turn == null || index < 0 || index >= cards.Count) return false;
            var card = cards[index];
            if (!turn.TrySpend(card.ActionCost)) return false;
            cards.RemoveAt(index);
            return true;
        }

        #endregion
    }
}
