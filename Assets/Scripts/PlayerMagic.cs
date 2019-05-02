using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMagic : MonoBehaviour
{
    private Animator anim;

    public AudioClip sfx;

    private Vector3 startPos;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.SendMessage("TakeDamage", Stats.Dmg);
        }
        Destroy(gameObject); 

    }

    void Start()
    {
        anim = GetComponent<Animator>();
        startPos = transform.position;
        // Ignore collisions with the player that spawned them. 
        Physics2D.IgnoreCollision(FindObjectOfType<Player>().GetComponent<Collider2D>(), GetComponent<Collider2D>());
        SoundManager.instance.PlaySingle(sfx);
        
    }

    private void Update()
    {
        if (transform.position.y > 4)
            Destroy(gameObject);

    }

    void FixedUpdate()
    {
        // When the projectile has travelled this many tiles away from the player, destroy it.
        if (Vector3.Distance(startPos, transform.position) > Stats.Range)
        {
            Destroy(gameObject);
        }

    }
}
