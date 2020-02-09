// Active item, send along a row or column to push enemies out of the way. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : Item
{

    // Keep hold of a reference to all the enemies for use in updating target later.
    GameObject[] enemies;
    GameObject pos;
    float moveTime = 0.1f;
    public AudioClip sfx;

    protected override void Pickup()
    {
        // Only pick up this item if not already owned by the player- prevent accidental pickup when used in a room. 
        if (Stats.active != this)
        {
            Stats.ActiveCharge = 4;
            Stats.CurrentCharge = 4;
            base.Pickup();
            gameObject.AddComponent<Rigidbody2D>();
            gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Smoothly move the player from one tile to the next. 
    private IEnumerator SmoothMovement(Vector3 end)
    {

        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        float inverseMoveTime = 1 / moveTime;

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPos = Vector3.MoveTowards(transform.position, end, inverseMoveTime * Time.deltaTime);
            transform.position = newPos;
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            // Wait until the next frame to continue execution. 
            yield return null;
        }

        gameObject.SetActive(false);
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy on hit of an obstacle. 
        if (collision.gameObject.tag == "Obstacles")
        {
            gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Enemy")
        {
            Vector2 targetTile;
            Vector2 startTile;
            RaycastHit2D hit;
            Vector3 translation = Vector3.zero;
            // For this enemy, determine which tiles either side of it are free. 
            // If only one is free, knock it to that tile. 
            // If both are free, randomise which to move it to. 
            // If neither are free, don't move. 
            // Sending along a row.
            if (Stats.Facing == Stats.Direction.LEFT || Stats.Facing == Stats.Direction.RIGHT)
            {
               
                startTile = collision.gameObject.transform.position;
                targetTile = startTile + new Vector2(0, 1);
                hit = Physics2D.Linecast(startTile, targetTile);

                if (!hit)
                    translation = new Vector3(0, 1, 0);

                targetTile = startTile + new Vector2(0, -1);
                hit = Physics2D.Linecast(startTile, targetTile);

                if (!hit)
                {
                    if (translation == Vector3.zero)
                        translation = new Vector3(0, -1, 0);
                    else if (RandomNumberGenerator.instance.Next() > 35)
                    {
                        translation = new Vector3(0, -1, 0);
                    }
                }
                
                // If a movement was found, translate the enemy there.
                if (translation != Vector3.zero)
                    collision.gameObject.transform.Translate(translation);
                else
                    gameObject.SetActive(false);
            }
            // Sending along a column. 
            else
            {
                startTile = collision.gameObject.transform.position;
                targetTile = startTile + new Vector2(1, 0);
                hit = Physics2D.Linecast(startTile, targetTile);

                if (!hit)
                    translation = new Vector3(1, 0, 0);

                targetTile = startTile + new Vector2(-1, 0);
                hit = Physics2D.Linecast(startTile, targetTile);

                if (!hit)
                {
                    if (translation == Vector3.zero)
                        translation = new Vector3(-1, 0, 0);
                    else if (RandomNumberGenerator.instance.Next() > 35)
                    {
                        translation = new Vector3(-1, 0, 0);
                    }
                }


                if (translation != Vector3.zero)
                    collision.gameObject.transform.Translate(translation);
                else
                    gameObject.SetActive(false);
            }
        }
    }


    public override void OnUse()
    {
        if (Stats.CurrentCharge == Stats.ActiveCharge)
        {
            // Reset the current amount of charge.
            Stats.CurrentCharge = 0;
            // Find the enemies and player for referencing this item's journey. 
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
            pos = GameObject.FindGameObjectWithTag("Player");

            // Send the item across the full length of the grid from here. 
            Vector3 end = Vector3.zero;
            switch (Stats.Facing)
            {
                case Stats.Direction.RIGHT:
                    end = new Vector3(3.5f, pos.transform.position.y, pos.transform.position.z);
                    break;
                case Stats.Direction.LEFT:
                    end = new Vector3(-3.5f, pos.transform.position.y, pos.transform.position.z);
                    break;
                case Stats.Direction.UP:
                    end = new Vector3(pos.transform.position.x, 3.5f, pos.transform.position.z);
                    break;
                case Stats.Direction.DOWN:
                    end = new Vector3(pos.transform.position.x, -3.5f, pos.transform.position.z);
                    break;
            }

            gameObject.transform.position = pos.transform.position;
            gameObject.SetActive(true);
            SoundManager.instance.PlaySingle(sfx);

            StartCoroutine(SmoothMovement(end));
        }

    }
    
}
