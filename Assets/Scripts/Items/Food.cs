// Heals the player by 1 full heart.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    protected override void Pickup()
    {
        // Edit the player's health. 
        Stats.Heal(1);
    }
}
