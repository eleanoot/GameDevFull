﻿// Base enemy class containing information and methods all enemy types will use. 

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
    protected float frozenTimer = 0f;

    // Reference to the sprite renderer to flash the sprite on hit. 
    protected Renderer rend;

    // Used to freeze the enemy from attacking when their HP is zero so the player can't be taken out by a technically defeated enemy unfairly. 
    protected bool defeated;
    protected bool frozen;

    // Used to ensure an enemy that should be at the borders of the grid actually is. 
    public bool edgeEnemy;

    public GameObject freezeInst;
    private GameObject f;

    // Start is called before the first frame update
    public void Start()
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
        // If the player has an effect on their attack, roll the dice to see if this takes effect.
        // Currently just seeing if this enemy gets frozen. 
        if (Stats.EffectChance > 0 && !frozen)
        {
            int result = Random.Range(1, 100);
            if (result <= Stats.EffectChance)
            {
                frozen = true;
                frozenTimer = 3.0f;
                SoundManager.instance.PlaySingle(SoundManager.instance.freezeClip);
                f = Instantiate(freezeInst, transform.position, Quaternion.AngleAxis(90, Vector3.forward));
            }
        }
        StartCoroutine(IsHit());
    }

    protected void UpdateTarget(Transform newTarget)
    {
        target = newTarget;
    }

    protected void Defeat()
    {
        TakeDamage((float)hp);
    }

    protected bool Unfreeze()
    {
        frozenTimer -= Time.deltaTime;
        if (frozenTimer <= 0.0f)
        {
            frozen = false;
            Destroy(f);
        }
        return frozen;
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
            if (Manager.instance != null)
                Stats.AddScore(Manager.instance.GetEnemyBonus());
            if (freezeInst != null)
            {
                Destroy(f);
            }
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
