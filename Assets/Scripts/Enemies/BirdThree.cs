using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdThree : Enemy
{
    // A reference to the projectile object they use.
    public GameObject feather;
    // How fast the projectiles travel. 
    public float featherSpeed;
    private float angle;

    void Awake()
    {
        // right.
        attackTargets = new Vector2Int[] { new Vector2Int(1, -1), new Vector2Int(1, 0), new Vector2Int(1, 1) };
        // The interval between attacks.
        actionTime = 1.5f;
        // Set a fractional delay to their starting time so not all instances of this enemy on the board are in sync.
        delayTime = (float)RandomNumberGenerator.instance.Next() / 100;
        delayTimer = delayTime;
        angle = -45.0f;

    }

    private new void Start()
    {
        // The default prefab for this enemy faces right, so if generation put it on the other side of the grid, flip everything. 
        if (Stats.TransformToGrid(gameObject.transform.position).x == 7)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            attackTargets[1] = new Vector2Int(-1, 0);
            attackTargets[0] = new Vector2Int(-1, 1);
            attackTargets[2] = new Vector2Int(-1, -1);
            angle = 135f;
        }
        base.Start();
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
                float currentAngle = angle;
                foreach (Vector2 n in attackTargets)
                {
                    GameObject magicInst = Instantiate(feather, transform.position, Quaternion.AngleAxis(currentAngle, Vector3.forward));
                    magicInst.transform.SetParent(transform);
                    magicInst.GetComponent<Rigidbody2D>().AddForce(n * featherSpeed);
                    // Apply the amount of health this enemy takes away to its projectile. 
                    magicInst.GetComponent<BasicProjectile>().SetDamage(damageDealt);
                    currentAngle += 45.0f;
                }
                // Reset the interval timer.
                attackTimer = 0f;
            }
        }

    }

    public override Vector2Int[] GetAttackTargets()
    {
        return new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1,-1) };
    }
}
