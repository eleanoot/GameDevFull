using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdMoving : Enemy
{
    // The interval between the enemy making a move. 
    public float movementTime;
    // How fast the projectiles travel. 
    public float featherSpeed;
    private float angle;

    private bool isMoving;

    void Awake()
    {
        // right.
        attackTargets = new Vector2Int[] { new Vector2Int(1, 0) };
        // The interval between attacks.
        actionTime = 1.5f;
        // Set a fractional delay to their starting time so not all instances of this enemy on the board are in sync.
        delayTime = (float)RandomNumberGenerator.instance.Next() / 100;
        delayTimer = delayTime;
        angle = 0f;
        isMoving = true;

    }

    private new void Start()
    {
        // The default prefab for this enemy faces right, so if generation put it on the other side of the grid, flip everything. 
        if (Stats.TransformToGrid(gameObject.transform.position).x == 7)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            attackTargets[0] = new Vector2Int(-1, 0);
            angle = 180f;
        }
        base.Start();

    }

    // Update is called once per frame
    void Update()
    {
        // Run down the delay timer before starting to attack. 
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }
        if (delayTimer <= 0f)
            Attack();
    }

    protected override void Attack()
    {
        if (attackTimer <= 0f)
            attackTimer = actionTime;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            // Only attack if the interval time has elapsed and this enemy isn't at 0 health. 
            if (attackTimer <= 0f && !defeated)
            {

                if (isMoving)
                {
                    // Choose a random row and move to it. 
                    int randomRow = RandomNumberGenerator.instance.Next();
                    int newY;
                    do
                    {
                        newY = (randomRow / 10) % 10;
                    } while (newY == Stats.TransformToGrid(transform.position).y);
                    
                    
                    StartCoroutine(SmoothMovement(new Vector2(transform.position.x, newY - 4 + 0.5f)));
                    
                    
                }
                else
                {
                    // Fire a projectile along that row.
                    // Loop through each tile this enemy attacks to fire a projectile in that direction. 

                    foreach (Vector2 n in attackTargets)
                    {
                        GameObject magicInst = ObjectPooler.instance.GetPooledObject("AirMagic");
                        if (magicInst != null)
                        {
                            magicInst.transform.position = gameObject.transform.position;
                            magicInst.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                            magicInst.transform.SetParent(transform);
                            magicInst.GetComponent<BasicProjectile>().SetDamage(damageDealt);
                            magicInst.SetActive(true);
                            magicInst.GetComponent<Rigidbody2D>().AddForce(n * featherSpeed);
                        }

                    }

                    isMoving = true;
                }


                
                // Reset the interval timer.
                attackTimer = 0f;
            }
        }

    }

    private IEnumerator SmoothMovement(Vector3 end)
    {
        Vector2 originalPos = transform.position;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        float inverseMoveTime = 1 / movementTime;

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPos = Vector3.MoveTowards(transform.position, end, inverseMoveTime * Time.deltaTime);
            transform.position = newPos;
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            // Wait until the next frame to continue execution. 
            yield return null;
        }
        isMoving = false;

    }


    public override Vector2Int[] GetAttackTargets()
    {
        return new Vector2Int[] { new Vector2Int(1, 0) };
    }
}
