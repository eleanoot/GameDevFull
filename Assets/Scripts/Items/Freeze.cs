﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freeze : Item
{
    protected override void Pickup()
    {
        Stats.EffectChance = 45;
        base.Pickup();
    }
}