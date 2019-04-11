// Enemy that stays in position on one tile and does nothing. 
// Intended to cause consideration of movement around the grid as stepping onto an enemy causes hp loss. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepingDog : Enemy
{

    // Start is called before the first frame update
    void Awake()
    {
        // Empty. Will only take up its own tile on the board. 
        attackTargets = new Vector2Int[] { };
    }
    

    public override Vector2Int[] GetAttackTargets()
    {
        return new Vector2Int[] { };
    }

    protected override void Attack()
    {
        // Nothing. He sleeps. 
        return;
    }
}
