using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicShield : Item
{
    public GameObject magicEffect;
    public AudioClip useSfx;

    protected override void Pickup()
    {
        // Only pick up this item if not already owned by the player- prevent accidental pickup when used in a room. 
        if (Stats.active != this)
        {
            Stats.ActiveCharge = 6;
            Stats.CurrentCharge = 6;
            base.Pickup();
            DontDestroyOnLoad(gameObject);
        }
    }

    public override void OnUse()
    {
        if (Stats.CurrentCharge == Stats.ActiveCharge)
        {
            // Reset the current amount of charge.
            Stats.CurrentCharge = 0;
            Stats.MagicShield = true;
            GameObject magicInst = Instantiate(magicEffect, FindObjectOfType<Player>().transform.position, Quaternion.identity);
            SoundManager.instance.PlaySingle(useSfx);
            magicInst.transform.SetParent(FindObjectOfType<Player>().transform);
        }

    }

    public override void OnRoomClear()
    {
        Stats.MagicShield = false;
    }
}
