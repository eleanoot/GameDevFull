// Increases the number of tiles away a player can direct to attack an enemy from by 1. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairyDust : Item
{
    protected override void Pickup()
    {
        // Edit the player's attack range. 
        Stats.Range *= 2;
        base.Pickup();
    }
    
}
