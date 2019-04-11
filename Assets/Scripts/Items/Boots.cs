// Gives the player a movement speed up of -0.1 to reduce their cooldown between actions. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boots : Item
{
    protected override void Pickup()
    {
        // Edit the player's movement speed. 
        Stats.Speed -= 0.1f;
        base.Pickup();
    }
}
