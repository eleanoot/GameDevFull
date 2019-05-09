using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetProjectile : MonoBehaviour
{
    private float damageDealt;
   // private Transform target;
    private Vector3 target;

    public AudioClip sfx;

    // Start is called before the first frame update
    void OnEnable()
    {
        // Ignore collisions with the enemy that spawned them. 
        if (transform.parent != null)
        {
            Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            SoundManager.instance.PlaySingle(sfx);
        }
            
        //var dir = target.position - transform.position;
        var dir = target - transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !Stats.MagicShield)
        {
            Stats.TakeDamage(damageDealt);
        }
        //else if (collision.gameObject.tag == "Item")
        //{
        //    target.gameObject.SendMessage("IsHit");
        //}
        //Destroy(gameObject); 
        gameObject.SetActive(false);
    }

    public void SetTarget(Vector3 t)
    {
        target = t;
    }

    public void SetDamage(float damage)
    {
        damageDealt = damage;
    }
}
