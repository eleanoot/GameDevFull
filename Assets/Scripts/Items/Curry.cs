// Replaces the player's attack with fireball magic.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curry : Item
{
    public GameObject magic;
    public Vector2Int[] magicTargets;

    protected override void Pickup()
    {
        Stats.Magic = magic;
        Stats.MagicSpeed = 200f;
        Stats.AttackSpeed = 0.3f;
        Stats.Dmg = 1.5f;
        Stats.Range = Mathf.Max(4, Stats.Range); // Set the pkayer's attack range to four at least.
        Stats.MagicTargets = magicTargets;
        base.Pickup();
    }
}
