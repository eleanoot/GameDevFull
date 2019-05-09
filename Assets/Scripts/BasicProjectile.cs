// Manages projectiles from magic based enemies.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{

    private float damageDealt;

    public AudioClip sfx;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !Stats.MagicShield)
        {
            Stats.TakeDamage(damageDealt);
        }
        //Destroy(gameObject); 
        gameObject.SetActive(false);

    }

    // Start is called before the first frame update
    void OnEnable()
    {
        // Ignore collisions with the enemy that spawned them. 
        if (transform.parent != null)
        {
            Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            SoundManager.instance.PlaySingle(sfx);
        }
            
    }

    public void SetDamage(float damage)
    {
        damageDealt = damage;
    }
}
