using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    //Card Visuals
    public string name {get; private set;}
    public string image {get; private set;}

    public Card (string name, string image) {
        this.name = name;
        this.image = image;
    }
}
