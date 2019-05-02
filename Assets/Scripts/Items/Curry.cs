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
        Mathf.Max(4, Stats.Range);
        Stats.MagicTargets = magicTargets;
        base.Pickup();
    }
}
