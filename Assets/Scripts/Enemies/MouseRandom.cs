// Starting version of the mouse enemy. Just moves randomly around the map and damages if moving onto the same tile. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseRandom : Enemy
{

    // The interval between the enemy making a move. 
    public float movementTime;

    // References to the tilemaps this enemy's pathfinding needs to avoid colliding with. 
    public Tilemap obstaclesTilemap;
    public Tilemap wallsTilemap;

    // Layer to check for other enemy collisions on. 
    public LayerMask unitsMask;
    
    // Use the same list reference for finding adjacent squares to save memory on creating new ones every frame. 
    List<Vector2Int> adjSquares = new List<Vector2Int>();

    // Reference to the animator on this enemy for movement. 
    private Animator anim;

    protected override void Attack()
    {
        if (attackTimer <= 0f)
            attackTimer = actionTime;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f && !defeated)
            {
                Vector2Int nextTile = ChooseNextMove();
                Move(nextTile.x, nextTile.y);
                attackTimer = actionTime;
            }
            
        }
    }

    private Vector2Int ChooseNextMove()
    {
        GetAdjacentSquares(Stats.TransformToGrid(transform.position));
        int randomIndex = RandomNumberGenerator.instance.Next();
        if (randomIndex >= adjSquares.Count)
        {
            randomIndex = ReduceNumber(randomIndex, adjSquares.Count);
        }
        return adjSquares[randomIndex];
    }

    private int ReduceNumber(int num, int max)
    {
        int result = num;
        do
        {
            int rhs = result % 10; // ones
            int lhs = (result / 10) % 10; // tens

            result = lhs + rhs;

            if (result <= 10)
            {
                result = Mathf.Abs(result - max);
            }
        } while (result >= max);

        return result;
    }


    // Works on grid spaces, not transform positions, to avoid errors due to floating point and the possiblity of running when the player is between tiles in smooth movement. 
    private void Move(float x, float y)
    {
        // Determine the direction the enemy has chosen to travel in based on the chosen tile to move to.
        float xDir = x - Stats.TransformToGrid(transform.position).x;
        float yDir = y - Stats.TransformToGrid(transform.position).y;

        // Change the walking animation direction depending on where we're turning. 
        // Currently indivudally setting and resetting every time to prevent a delay. 
        if (xDir < 0)
        {
            anim.SetTrigger("turnLeft");
            anim.ResetTrigger("turnRight");
            anim.ResetTrigger("turnUp");
            anim.ResetTrigger("turnDown");
        }
        else if (xDir > 0)
        {
            anim.SetTrigger("turnRight");
            anim.ResetTrigger("turnLeft");
            anim.ResetTrigger("turnUp");
            anim.ResetTrigger("turnDown");
        }
        else if (yDir > 0)
        {
            anim.SetTrigger("turnUp");
            anim.ResetTrigger("turnRight");
            anim.ResetTrigger("turnLeft");
            anim.ResetTrigger("turnDown");
        }
        else if (yDir < 0)
        {
            anim.SetTrigger("turnDown");
            anim.ResetTrigger("turnRight");
            anim.ResetTrigger("turnUp");
            anim.ResetTrigger("turnLeft");
        }

        // Does this next tile contain the target?
        if (x == Stats.TransformToGrid(target.transform.position).x && y == Stats.TransformToGrid(target.transform.position).y)
        {
            // Action on hit depends if the target is still the player or not. For attacking items like the decoy.  
            if (target.transform != GameObject.FindGameObjectWithTag("Player").transform)
            {
                target.gameObject.SendMessage("IsHit");
            }
            else if (!Stats.MeleeShield)
            {
                if (Stats.TakeDamage(damageDealt))
                    StartCoroutine(GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().IsHit()); // only flash the sprite if damage has actually been taken: prevent overlap of player being left invisible on final damage hit. 
            }
        }
        // Otherwise move into this tile. 
        else
        {
            Vector2 startTile = transform.position;
            Vector2 targetTile = new Vector2(x - 4 + 0.5f, y - 4 + 0.5f);

            StartCoroutine(SmoothMovement(targetTile));
        }
    }

    // Smoothly move the enemy from one tile to the next like the player.
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

        // If the mouse and the player somehow ended up on the same square at the same time after this move, move the mouse back one. 
        if (Stats.TransformToGrid(target.transform.position) == Stats.TransformToGrid(transform.position))
        {
            StartCoroutine(GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().IsHit());
            Vector2 startTile = transform.position;
            Vector2 targetTile = originalPos;
            StartCoroutine(SmoothMovement(targetTile));
        }

        // If this mouse ended up on the same position as another mouse after this move, push it back. 
        this.GetComponent<BoxCollider2D>().enabled = false;
        RaycastHit2D hit = Physics2D.Linecast(transform.position, transform.position, unitsMask);
        this.GetComponent<BoxCollider2D>().enabled = true;
        if (hit.transform != null && (hit.transform.gameObject.tag == "Enemy"))
        {
            Vector2 startTile = transform.position;
            Vector2 targetTile = originalPos;
            StartCoroutine(SmoothMovement(targetTile));
        }
    }

    private void GetAdjacentSquares(Vector2Int p)
    {
        adjSquares.Clear();
        foreach (Vector2Int d in attackTargets)
        {
            int x = p.x + d.x;
            int y = p.y + d.y;
            // If the tile under consideration is within the grid and doesn't contain a non traversable board element, it's a valid adjacent tile. 
            if (x >= -1 && x <= 8 && y >= -1 && y <= 8
                && !CheckForCollision(new Vector2(p.x - 4 + 0.5f, p.y - 4 + 0.5f), new Vector2(x - 4 + 0.5f, y - 4 + 0.5f)))
            {
                adjSquares.Add(new Vector2Int(x, y));
            }
        }
    }

    // Need to check for collisions with both enemies, obstacles, and the outer walls.
    // Obstacles tested by tilemap check, enemies by raycast.
    private bool CheckForCollision(Vector2 start, Vector2 end)
    {
        bool hasObstacle = getCell(obstaclesTilemap, end) != null;
        bool hasWall = getCell(wallsTilemap, end) != null;

        // Disable this enemy's box collider so as not to collide with itself in raycast. 
        this.GetComponent<BoxCollider2D>().enabled = false;
        RaycastHit2D hit = Physics2D.Linecast(start, end, unitsMask);
        this.GetComponent<BoxCollider2D>().enabled = true;
        if (hit.transform != null && (hit.transform != target.transform))
        {
            return true;
        }
        else if (hasObstacle || hasWall)
            return true;
        return false;
    }

    private void Awake()
    {
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        obstaclesTilemap = tilemaps[0];
        wallsTilemap = tilemaps[2];
        anim = GetComponent<Animator>();
        // right, up, left, down, 
        attackTargets = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };

        actionTime = 0.5f;
        attackTimer = actionTime;
        delayTime = (float)RandomNumberGenerator.instance.Next() / 100;
        delayTimer = delayTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }
        if (delayTimer <= 0f)
            Attack();
    }

    /* TILEMAP/MOVEMENT UTILS */
    // Determine if the given cell position is part of this tilemap or not.
    private TileBase getCell(Tilemap tilemap, Vector2 cellWorldPos)
    {
        return tilemap.GetTile(tilemap.WorldToCell(cellWorldPos));
    }

    public override Vector2Int[] GetAttackTargets()
    {
        return new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
    }

}
