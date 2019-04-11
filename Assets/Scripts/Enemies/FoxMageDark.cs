// Variant of FoxMage. Stands still on the board and fires projectiles in the ordinal directions. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxMageDark : Enemy
{
    // The projectile this enemy uses. 
    public GameObject magic;
    // How fast the projectile travel. 
    public float magicSpeed;


    void Awake()
    {
        // north east, north west, south west, south east
        attackTargets = new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1) };
        actionTime = 1.5f;
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
            // Only attack if the interval has elapsed and this enemy has not run out of HP. 
            if (attackTimer <= 0f && !defeated)
            {
                float angle = 0f;
                foreach (Vector2 n in attackTargets)
                {
                    GameObject magicInst = Instantiate(magic, transform.position, Quaternion.AngleAxis(angle, Vector3.forward));
                    magicInst.transform.SetParent(transform);
                    magicInst.GetComponent<Rigidbody2D>().AddForce(n * magicSpeed);
                    // Apply the damage this enemy does to the projectiles it fires. 
                    magicInst.GetComponent<Project>().SetDamage(damageDealt);
                    angle += 90f;
                }

                attackTimer = 0f;
            }
        }

    }

    public override Vector2Int[] GetAttackTargets()
    {
        return new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1) };
    }

}
