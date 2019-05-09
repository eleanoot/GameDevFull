using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMagic : MonoBehaviour
{
    private Animator anim;

    public AudioClip fireball1;
    public AudioClip fireball2;

    private Vector3 startPos;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.SendMessage("TakeDamage", Stats.Dmg);
        }
        //Destroy(gameObject); 
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        anim = GetComponent<Animator>();
        startPos = transform.position;
        // Ignore collisions with the player that spawned them. 
        Physics2D.IgnoreCollision(FindObjectOfType<Player>().GetComponent<Collider2D>(), GetComponent<Collider2D>());
        if (Stats.RoomCount > 1) // prevent noise from playing on pool instantiation since the player shouldn't need magic in these early rooms
            SoundManager.instance.RandomizeSfx(0.7f, fireball1, fireball2);
        
    }

    private void Update()
    {
        if (transform.position.y > 4)
            //Destroy(gameObject);
            gameObject.SetActive(false);

    }

    void FixedUpdate()
    {
        // When the projectile has travelled this many tiles away from the player, destroy it.
        if (Vector3.Distance(startPos, transform.position) > Stats.Range)
        {
            //Destroy(gameObject);
            gameObject.SetActive(false);
        }

    }
}
