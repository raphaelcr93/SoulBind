using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Player : ITurnPlayer, ICardGamePlayer, IResourceGamePlayer
{
    public string playerToken { get; private set; }
    public float cameraZRotation { get; private set; }

    //ITurnPlayer Interface

    public string playerName { get; private set; }

    public void OnTurnStart()
    {
        //throw new System.NotImplementedException();
    }

    public void OnTurnEnd()
    {
        //throw new System.NotImplementedException();
    }

    //ICardGamePlayer Interface
    public List<Card> cardsOnHand { get; private set; }
    public List<Card> cardsOnDeck { get; private set; }
    public List<Card> cardsOnGraveyard { get; private set; }
    public List<Card> originalDeck { get; private set; }

    public void DrawCard()
    {
        var count = cardsOnDeck.Count;
        if (count > 0)
        {
            Card card = cardsOnDeck[count - 1];
            cardsOnDeck.Remove(card);
            cardsOnHand.Add(card);
        }
    }

    public void DiscarCardFromHand(Card cardToDiscard)
    {
        if (cardsOnHand.Contains(cardToDiscard))
        {
            cardsOnHand.Remove(cardToDiscard);
            cardsOnGraveyard.Add(cardToDiscard);
        }
    }

    public void ShuffleDeck()
    {
        var count = cardsOnDeck.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = cardsOnDeck[i];
            cardsOnDeck[i] = cardsOnDeck[r];
            cardsOnDeck[r] = tmp;
        }
    }

    public void ResetDeck()
    {
        cardsOnDeck = originalDeck;
    }

    // IResourceGamePlayer Interface
    public List<ResourceValue> resources { get; private set; }

    private ResourceValue GetResourceValueObject(ResourceUtils.ResourceTypes type)
    {
        return resources.Find(x => x.type == type);
    }

    public void AddResourcePoints(ResourceUtils.ResourceTypes type, int value)
    {
        GetResourceValueObject(type).AddPoints(value);
    }

    public void RemoveResourcePoints(ResourceUtils.ResourceTypes type, int value)
    {
        GetResourceValueObject(type).RemovePoints(value);
    }

    public int GetResourceValue(ResourceUtils.ResourceTypes type)
    {
        return GetResourceValueObject(type).points;
    }

    public Player(string playerToken,  string playerName, float cameraZRotation, List<Card> cardsOnDeck)
    {
        this.playerToken = playerToken;
        this.playerName = playerName;
        this.cameraZRotation = cameraZRotation;
        this.originalDeck = cardsOnDeck;
        this.cardsOnHand = new List<Card>();
        this.cardsOnGraveyard = new List<Card>();
        ResetDeck();

        this.resources = new List<ResourceValue>();
        foreach (ResourceUtils.ResourceTypes item in Enum.GetValues(typeof(ResourceUtils.ResourceTypes)).Cast<ResourceUtils.ResourceTypes>())
        {
            ResourceValue resourceValue = new ResourceValue(item, 0);
            resources.Add(resourceValue);
        }
    }

}


public interface ICardGamePlayer
{
    List<Card> originalDeck { get; }
    List<Card> cardsOnHand { get; }
    List<Card> cardsOnDeck { get; }
    List<Card> cardsOnGraveyard { get; }

    public abstract void DrawCard();
    public abstract void DiscarCardFromHand(Card cardToDiscard);
    public abstract void ShuffleDeck();
    public abstract void ResetDeck();
}

public interface IResourceGamePlayer
{
    List<ResourceValue> resources { get; }

    public abstract void AddResourcePoints(ResourceUtils.ResourceTypes type, int value);
    public abstract void RemoveResourcePoints(ResourceUtils.ResourceTypes type, int value);
    public abstract int GetResourceValue(ResourceUtils.ResourceTypes type);
}

public class ResourceValue
{
    public ResourceUtils.ResourceTypes type { get; private set; }
    public int points { get; private set; }

    public ResourceValue(ResourceUtils.ResourceTypes type, int points)
    {
        this.type = type;
        this.points = points;
    }

    public void AddPoints(int value)
    {
        points += value;
    }

    public void RemovePoints(int value)
    {
        points -= value;
    }
}

public class ResourceUtils
{
    //Possible Types of control actions. This shouldn't change
    public enum ResourceTypes
    {
        MovePoints,
        AttackPoints,
        InvocationPonints,
        MagicPoints,
        AbilityPoints
    }
}