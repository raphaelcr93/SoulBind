using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardServer
{
    public string cardName;
    public string cardDescription;
    public string cardImagePath; // Path to the card image

    public CardServer(string name, string description, string imagePath)
    {
        cardName = name;
        cardDescription = description;
        cardImagePath = imagePath;
    }
}



