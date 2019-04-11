// Enemy that stands still on the grid and fires magic across two spaces in the cardinal directions. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxMage : Enemy
{
    // A reference to the projectile object they use.
    public GameObject magic;
    // How fast the projectiles travel. 
    public float magicSpeed;
    

    void Awake()
    {
        // right, up, left, down. In this order so the loop rotating through each attack direction works correctly at each 90 degrees. 
        attackTargets = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
        // The interval between attacks.
        actionTime = 1.5f;
        // Set a fractional delay to their starting time so not all instances of this enemy on the board are in sync.
        delayTime = (float)RandomNumberGenerator.instance.Next() / 100;
        delayTimer = delayTime;
    }

    void Update()
    {
        // Run down the delay timer before starting to attack. 
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }
        if (delayTimer <= 0f)
            Attack();
        
    }

    protected override void Attack()
    {
        if (attackTimer <= 0f)
            attackTimer = actionTime;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            // Only attack if the interval time has elapsed and this enemy isn't at 0 health. 
            if (attackTimer <= 0f && !defeated)
            {
                // Loop through each tile this enemy attacks to fire a projectile in that direction. 
                float angle = 0f;
                foreach (Vector2 n in attackTargets)
                {
                    GameObject magicInst = Instantiate(magic, transform.position, Quaternion.AngleAxis(angle, Vector3.forward));
                    magicInst.transform.SetParent(transform);
                    magicInst.GetComponent<Rigidbody2D>().AddForce(n * magicSpeed);
                    // Apply the amount of health this enemy takes away to its projectile. 
                    magicInst.GetComponent<Project>().SetDamage(damageDealt);
                    angle += 90f;
                }
                // Reset the interval timer.
                attackTimer = 0f;
            }
        }

    }

    public override Vector2Int[] GetAttackTargets()
    {
        return new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
    }
}
