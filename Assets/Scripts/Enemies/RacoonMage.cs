// Selects the tile the player is currently on and aims for it.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacoonMage : Enemy
{
    // How fast the projectiles travel. 
    public float magicSpeed;

    void Awake()
    {
        // right, up, left, down. In this order so the loop rotating through each attack direction works correctly at each 90 degrees. 
        attackTargets = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
        // The interval between attacks.
        actionTime = 2.0f;
        // Set a fractional delay to their starting time so not all instances of this enemy on the board are in sync.
        delayTime = (float)RandomNumberGenerator.instance.Next() / 100;
        delayTimer = delayTime;
    }

    // Update is called once per frame
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
        if (frozen)
        {
            if (Unfreeze())
                return;

        }
        if (attackTimer <= 0f)
            attackTimer = actionTime;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            // Only attack if the interval time has elapsed and this enemy isn't at 0 health. 
            if (attackTimer <= 0f && !defeated)
            {
                Vector2 attackDir = target.position - transform.position;
                //GameObject magicInst = Instantiate(magic, transform.position, Quaternion.identity);
                //magicInst.transform.SetParent(transform);
                //magicInst.GetComponent<TargetProjectile>().SetTarget(target.position);
                //magicInst.GetComponent<Rigidbody2D>().AddForce(attackDir.normalized * magicSpeed);
                //// Apply the amount of health this enemy takes away to its projectile. 
                //magicInst.GetComponent<TargetProjectile>().SetDamage(damageDealt);

                GameObject magicInst = ObjectPooler.instance.GetPooledObject("TargetMagic");
                if (magicInst != null)
                {
                    magicInst.transform.position = gameObject.transform.position;
                    magicInst.transform.SetParent(transform);
                    magicInst.GetComponent<TargetProjectile>().SetTarget(target.position);
                    magicInst.GetComponent<TargetProjectile>().SetDamage(damageDealt);
                    magicInst.SetActive(true);
                    magicInst.GetComponent<Rigidbody2D>().AddForce(attackDir.normalized * magicSpeed);
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
