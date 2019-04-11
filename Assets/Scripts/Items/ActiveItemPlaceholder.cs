// A placeholder to demonstrate the picking up/switching and charge of active items. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveItemPlaceholder : Item
{
    protected override void Pickup()
    {
        if (Stats.active != this)
        {
            Stats.ActiveCharge = 4;
            Stats.CurrentCharge = 4;
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
        }
        
    }
}
