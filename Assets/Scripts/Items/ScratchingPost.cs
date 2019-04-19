using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScratchingPost : Item
{
    protected override void Pickup()
    {
        // Edit the player's health. 
        Stats.Dmg++;
    }
}
