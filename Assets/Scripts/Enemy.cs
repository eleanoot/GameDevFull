// Base enemy class containing information and methods all enemy types will use. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherits from base class for objects that can move. 
public abstract class Enemy : MonoBehaviour
{
    // How much HP this enemy has left.
    public double hp;
    // How many hearts this enemy's attack knocks off the player.
    public float damageDealt;
    // The squares this enemy aims for, if any. Used in calculation of spaces taken up on the grid.
    protected Vector2Int[] attackTargets;
    // The particular unit this enemy aims for, if any attack direction: by default its the player. 
    protected Transform target;
    // The time it takes this Enemy to act. 
    protected float actionTime;
    // Delay for this enemy to begin looping attacks. Used only once. 
    protected float delayTime;
    protected float attackTimer = 0f;
    protected float delayTimer = 0f;

    // Reference to the sprite renderer to flash the sprite on hit. 
    private Renderer rend;

    // Used to freeze the enemy from attacking when their HP is zero so the player can't be taken out by a technically defeated enemy unfairly. 
    protected bool defeated;
    

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        defeated = false;
    }

    // Will be overridden by functons in the inherited classes that specialise the enemy type.
    protected abstract void Attack();
    

    void TakeDamage(float dmg)
    {
        hp -= dmg;
        StartCoroutine(IsHit());
    }

    protected void UpdateTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // 'Flash' the sprite when attacked.
    private IEnumerator IsHit()
    {
        if (hp <= 0)
            defeated = true; 
        for (int i = 0; i < 5; i++)
        {
            rend.enabled = true;
            yield return new WaitForSeconds(0.1f);
            rend.enabled = false;
            yield return new WaitForSeconds(0.1f);
        }
        rend.enabled = true;

        if (hp <= 0)
        {
            Stats.AddScore(Manager.instance.GetEnemyBonus());
            Destroy(gameObject);
        }
            
    }

    // Must be implemented specifically by the enemy to calculate before instantiation.
    public abstract Vector2Int[] GetAttackTargets();

    public bool IsDefeated()
    {
        return defeated;    
    }
}
