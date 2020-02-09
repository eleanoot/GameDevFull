// Replace the player's attack with fireballs that move in the cardinal directions. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : Item
{
    public GameObject magic;
    public Vector2Int[] magicTargets;
    public float angle;

    protected override void Pickup()
    {
        Stats.Magic = magic;
        Stats.MagicSpeed = 200f;
        Stats.MagicAngle = angle;
        Stats.AttackSpeed = 0.3f;
        Stats.Dmg = 1.0f;
        Stats.Range = Mathf.Max(2, Stats.Range);
        Stats.MagicTargets = magicTargets;
        base.Pickup();
    }
}
