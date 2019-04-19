// Manages projectiles from magic based enemies.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{

    private float damageDealt;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Stats.TakeDamage(damageDealt);
        }
        Destroy(gameObject); // TODO: save memory in full version by switching to object pooling and reuse the projectiles. 

    }

    // Start is called before the first frame update
    void Start()
    {
        // Ignore collisions with the enemy that spawned them. 
        Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        
    }

    public void SetDamage(float damage)
    {
        damageDealt = damage;
    }
}
