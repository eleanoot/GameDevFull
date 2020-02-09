// Manages projectiles from magic based enemies.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Project : MonoBehaviour
{
    private Animator anim;

    private float damageDealt;
    
    public AudioClip fireball1;
    public AudioClip fireball2;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !Stats.MagicShield)
        {
            Stats.TakeDamage(damageDealt);
        }
        
        gameObject.transform.parent = null;
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        anim = GetComponent<Animator>();
        // Ignore collisions with the enemy that spawned them. 
        if (transform.parent != null)
        {
            Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            SoundManager.instance.RandomizeSfx(0.3f, fireball1, fireball2);
        }
            
;    }
    
    void FixedUpdate()
    {
        // When the projectile has travelled this many tiles away from the enemy, destroy it.
        if (Vector3.Distance(transform.parent.position, transform.position) > 2)
        {
            gameObject.transform.parent = null;
            gameObject.SetActive(false);
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
