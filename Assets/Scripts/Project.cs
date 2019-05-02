﻿// Manages projectiles from magic based enemies.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Project : MonoBehaviour
{
    private Animator anim;

    private float damageDealt;

    public AudioClip sfx;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !Stats.MagicShield)
        {
            Stats.TakeDamage(damageDealt);
        }
        Destroy(gameObject); 

    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        // Ignore collisions with the enemy that spawned them. 
        Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        SoundManager.instance.PlaySingle(sfx);
;    }
    
    void FixedUpdate()
    {
        // When the projectile has travelled this many tiles away from the enemy, destroy it.
        if (Vector3.Distance(transform.parent.position, transform.position) > 2)
        {
            Destroy(gameObject);
        }
        // Halfway through travel reduce to a smaller sprite for 'fading out'.
        else if (Vector3.Distance(transform.parent.position, transform.position) > 1)
        {
            anim.Play("MagicOrangeFade");
        }

    }

    public void SetDamage(float damage)
    {
        damageDealt = damage;
    }
}
