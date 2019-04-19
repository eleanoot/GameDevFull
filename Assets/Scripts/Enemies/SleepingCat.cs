// Stays on one tile and does nothing until player moves within a number of tiles, then chases them.
// States: sleeping -> attack 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SleepingCat : Enemy
{
    // The interval between the enemy making a move. 
    public float movementTime;

    // References to the tilemaps this enemy's pathfinding needs to avoid colliding with. 
    public Tilemap obstaclesTilemap;
    public Tilemap wallsTilemap;

    // Layer to check for other enemy collisions on. 
    public LayerMask unitsMask;

    // The best path created by the pathfinding algorithm to the target this turn. 
    private List<Path> bestPath = new List<Path>();
    // Use the same list reference for finding adjacent squares to save memory on creating new ones every frame. 
    List<Path> adjSquares = new List<Path>();
    private Vector2Int[] checkTargets = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1) };

    // Reference to the animator on this enemy for movement. 
    private Animator anim;

    bool awakeState;

    // Start is called before the first frame update
    void Awake()
    {
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        obstaclesTilemap = tilemaps[0];
        wallsTilemap = tilemaps[2];
        anim = GetComponent<Animator>();
        // right, up, left, down, 
        attackTargets = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };

        actionTime = 0.75f;
        attackTimer = actionTime;
        delayTime = (float)RandomNumberGenerator.instance.Next() / 100;
        delayTimer = delayTime;

        awakeState = false;
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

    // Two states of attack: doing nothing and trying to chase after the player. 
    // Sleeping: checks tiles around for if the player has entered its range and transitions if so. 
    // Attack: chases after the player. Cannot go back into sleeping state. 
    protected override void Attack()
    {
        if (!awakeState)
        {
            // Check tiles around for the player. 
            if (PlayerNearby())
            {
                anim.SetBool("awakeState", true);
                awakeState = true;
            }
                
        }
        else
        {
            // Attack state is on the timer to introduce the usual delay. 
            if (attackTimer <= 0f)
                attackTimer = actionTime;

            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
                // Only attack if the interval time is up and the enemy's hp isn't 0. 
                if (attackTimer <= 0f && !defeated)
                {
                    // Run the pathfinding algorithm towards the target to find the next move. 
                    HappyPath();
                    // Ideally the path to the player should never be blocked off so best path should never be empty, but just in case. 
                    if (bestPath.Count > 0)
                    {
                        // Pop the first tile off the top of the best path for the next move. 
                        Path nextMove = bestPath[0];
                        bestPath.RemoveAt(0);
                        Move(nextMove.x, nextMove.y);
                    }

                    attackTimer = actionTime;
                }
            }
        }


        
    }

    private bool PlayerNearby()
    {
        Vector2Int playerPos = Stats.TransformToGrid(target.transform.position);
        foreach (Vector2Int c in checkTargets)
        {
            if (c + Stats.TransformToGrid(transform.position) == playerPos)
                return true;
        }
        return false;
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
            else if (!Stats.Shield)
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

    // Find the possible adjacent squares for the enemy to move to next in pathfinding. 
    private void GetAdjacentSquares(Path p)
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
                adjSquares.Add(new Path(p.g + 1, ManhattanDistance(new Vector2(x, y), Stats.TransformToGrid(new Vector2(target.position.x, target.position.y))), p, x, y));
            }
        }
    }

    // Manhattan distance heuristic for A* pathfinding's h value. 
    public static int ManhattanDistance(Vector2 a, Vector2 b)
    {
        return Mathf.RoundToInt(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
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

    // A* pathfinding - find the tile with the lowest movement distance out of the current squares to choose from. 
    private static Path FindLowestFScore(List<Path> openList)
    {
        int lowestF = int.MaxValue;
        Path lowestCell = null;
        foreach (Path p in openList)
        {
            if (p.f < lowestF)
            {
                lowestF = p.f;
                lowestCell = p;
            }
        }
        return lowestCell;
    }

    // A* pathfinding - find the shortest path to the target. Works on integer grid coordinates over floating point transforms to avoid errors 
    // while the player is moving between tiles. 
    private void HappyPath()
    {
        Path destinationSquare = new Path(0, 0, null, Stats.TransformToGrid(target.position).x, Stats.TransformToGrid(target.position).y);

        List<Path> evaluationList = new List<Path>();
        List<Path> closedPathList = new List<Path>();
        // Add the starting tile to the evaluation list. 
        evaluationList.Add(new Path(0, 0, null, Stats.TransformToGrid(gameObject.transform.position).x, Stats.TransformToGrid(gameObject.transform.position).y));
        Path currentSquare = null;
        while (evaluationList.Count > 0)
        {
            currentSquare = FindLowestFScore(evaluationList);
            closedPathList.Add(currentSquare);
            evaluationList.Remove(currentSquare);
            // The target has been located or infinite loop break for security for now.
            if (DestinationFound(closedPathList, destinationSquare) || closedPathList.Count > 64)
            {
                // Build the path in reverse from the destination square and return it for the next movement consideration. 
                bestPath = BuildPath(currentSquare);
                break;
            }
            // Fill in the current adjacent traversable tiles around this position.
            GetAdjacentSquares(currentSquare);
            foreach (Path p in adjSquares)
            {
                if (closedPathList.Contains(p))
                    continue; // skip this one, we already know about it
                if (!evaluationList.Contains(p))
                {
                    evaluationList.Add(p);
                }
                // If its a shorter path to reach this tile from the current direction, update its scores and parent. 
                else if (p.h + currentSquare.g + 1 < p.f)
                    p.parent = currentSquare;
            }
        }



    }

    // A* pathfinding - determine if the closed list contains our wanted tile. Own function needed to compare the relevant parts of the Path class.
    private bool DestinationFound(List<Path> closed, Path dest)
    {
        foreach (Path p in closed)
        {
            if (dest.x == p.x && dest.y == p.y)
                return true;

        }
        return false;
    }

    // Simply used because at the end of our loop we have a path with parents in the reverse order. This reverses the list so it's from Enemy to Player
    private List<Path> BuildPath(Path p)
    {
        List<Path> bestPath = new List<Path>();
        Path currentLoc = p;
        bestPath.Insert(0, currentLoc);
        while (currentLoc.parent != null)
        {
            currentLoc = currentLoc.parent;
            if (currentLoc.parent != null)
                bestPath.Insert(0, currentLoc);
        }
        return bestPath;
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
