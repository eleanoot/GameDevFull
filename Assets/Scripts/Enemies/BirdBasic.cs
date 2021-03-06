﻿// Enemy that stands still on the grid and fires magic across two spaces in the cardinal directions. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBasic : Enemy
{
    // How fast the projectiles travel. 
    public float featherSpeed;
    private float angle;

    // Reference to the animator on this enemy for movement. 
    private Animator anim;

    void Awake()
    {
        // right.
        attackTargets = new Vector2Int[] { new Vector2Int(1, 0) };
        // The interval between attacks.
        actionTime = 1.5f;
        // Set a fractional delay to their starting time so not all instances of this enemy on the board are in sync.
        delayTime = (float)RandomNumberGenerator.instance.Next() / 100;
        delayTimer = delayTime;
        angle = 0f;
        anim = GetComponent<Animator>();
    }

    private new void Start()
    {
        // The default prefab for this enemy faces right, so if generation put it on the other side of the grid, flip everything. 
        if (Stats.TransformToGrid(gameObject.transform.position).x == 7)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            attackTargets[0] = new Vector2Int(-1, 0);
            angle = 180f;
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
        if (frozen)
        {
            if (!Unfreeze())
                anim.speed = 1f;
            else
            {
                anim.speed = 0f;
                return;
            }
            
        }

        if (attackTimer <= 0f)
            attackTimer = actionTime;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            // Only attack if the interval time has elapsed and this enemy isn't at 0 health. 
            if (attackTimer <= 0f && !defeated)
            {
                // Loop through each tile this enemy attacks to fire a projectile in that direction. 
                
                foreach (Vector2 n in attackTargets)
                {
                    GameObject magicInst = ObjectPooler.instance.GetPooledObject("AirMagic");
                    if (magicInst != null)
                    {
                        magicInst.transform.position = gameObject.transform.position;
                        magicInst.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                        magicInst.transform.SetParent(transform);
                        magicInst.GetComponent<BasicProjectile>().SetDamage(damageDealt);
                        magicInst.SetActive(true);
                        magicInst.GetComponent<Rigidbody2D>().AddForce(n * featherSpeed);
                    }

                }
                // Reset the interval timer.
                attackTimer = 0f;
            }
        }

    }

    public override Vector2Int[] GetAttackTargets()
    {
        return new Vector2Int[] { new Vector2Int(1, 0) };
    }
}
