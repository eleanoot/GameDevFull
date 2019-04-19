// Manages control of the player character. 

using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    // Tilemap references to check for collisions. 
    public Tilemap groundTilemap;
    public Tilemap wallsTilemap;
    public Tilemap obstaclesTilemap;

    public AudioClip hurtSfx;
    public AudioClip scratchSfx;
    public AudioClip steps1Sfx;
    public AudioClip steps2Sfx;
    public AudioClip blockedSfx;

    // The layer for which to check for collider collisions with. 
    public LayerMask unitsMask;

    // How long the smooth movement will take.
    private float moveTime = 0.1f;
    // Booleans to check if we're currently transitioning and shouldn't be updating yet.
    public bool isMoving = false;
    public bool onCooldown = false;
    

    // Reference to the animation controller for the player to change walking animation direction.
    private Animator anim;
    // Reference to the sprite renderer to flash the sprite on hit. 
    private Renderer rend;
    private BoxCollider2D boxCollider;

    void Start()
    {
        gameObject.transform.SetPositionAndRotation(Stats.LastPos, Quaternion.identity);
        anim = GetComponent<Animator>();
        rend = GetComponent<Renderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // If the player is still moving or we're paused, do nothing this update.
        if (isMoving || onCooldown || Time.timeScale == 0)
            return;
       
        // Get the move direction.
        int horizontal = (int)Input.GetAxisRaw("Horizontal");
        int vertical = (int)Input.GetAxisRaw("Vertical");

        // Don't want to be allowed to move diagonally on the grid.
        if (horizontal != 0)
            vertical = 0;

        // If a direction has been provided this update, we're trying to move. 
        if (horizontal != 0 || vertical != 0)
        {
            // Start some delay between movements to prevent going too fast.
            StartCoroutine(ActionCooldown(Stats.Speed));
            MovePlayer(horizontal, vertical);
        }
    }

    // Attacking checked for in late update after any possible movement has occured- a player might try to quickly move next to an enemy, attack, then move back out of range. 
    // This won't register if both actions are in the same frame due to the coroutine.
    private void LateUpdate()
    {
        // Do nothing if paused.
        if (Time.timeScale == 0)
            return;

        // Use active item. 
        if (Input.GetKeyDown("space"))
        {
            if (Stats.active != null && Stats.RoomCount % 5 != 0) // Disable using active items in item rooms due to them being accidentally picked up again and resetting charge.
            {
                Stats.active.OnUse();
            }
        }
        
        // Attack in any direction, not just the current one being faced. 
        if (Input.GetKeyDown("w"))
        {
            Attack('w');
        }
        else if (Input.GetKeyDown("a"))
        {
            Attack('a');
        }
        else if (Input.GetKeyDown("s"))
        {
            Attack('s');
        }
        else if (Input.GetKeyDown("d"))
        {
            Attack('d');
        }
    }

    private void Attack(char key)
    {
        int xDir = 0;
        int yDir = 0;
       switch (key)
        {
            case 'a':
                xDir = -1;
                anim.SetTrigger("turnLeft");
                anim.ResetTrigger("turnRight");
                anim.ResetTrigger("turnUp");
                anim.ResetTrigger("turnDown");
                Stats.Facing = Stats.Direction.LEFT;
                break;
            case 'd':
                xDir = 1;
                anim.SetTrigger("turnRight");
                anim.ResetTrigger("turnLeft");
                anim.ResetTrigger("turnUp");
                anim.ResetTrigger("turnDown");
                Stats.Facing = Stats.Direction.RIGHT;
                break;
            case 'w':
                yDir = 1;
                anim.SetTrigger("turnUp");
                anim.ResetTrigger("turnRight");
                anim.ResetTrigger("turnLeft");
                anim.ResetTrigger("turnDown");
                Stats.Facing = Stats.Direction.UP;
                break;
            case 's':
                yDir = -1;
                anim.SetTrigger("turnDown");
                anim.ResetTrigger("turnRight");
                anim.ResetTrigger("turnUp");
                anim.ResetTrigger("turnLeft");
                Stats.Facing = Stats.Direction.DOWN;
                break;
        }

        Vector2 startTile = transform.position;
        Vector2 targetTile = startTile + new Vector2(xDir, yDir);
        // Raycast to check if an enemy prefab is on this tile. Cast a line from the start point to the end point on the Units layer.
        // Disable the boxCollider so that linecast doesn't hit this object's own collider.
        //boxCollider.enabled = false;
        // Set target tile for enemy spaces based on the player's current range.
        Vector2 rangeTile = startTile + new Vector2(xDir * Stats.Range, yDir * Stats.Range);
        RaycastHit2D hit = Physics2D.Linecast(startTile, rangeTile, unitsMask);
        // Re-enable boxCollider after linecast
        //boxCollider.enabled = true;
        if (hit.transform != null && hit.transform.gameObject.tag == "Enemy")
        {
            // If the player is attacking from a distance, 'pounce' to a tile in front of the enemy. Otherwise just attack from where standing.  
            if (Stats.Range > 1 && hit.distance >= 1.0)
            {
                StartCoroutine(SmoothMovement(rangeTile - new Vector2(xDir, yDir)));
            }
            hit.transform.gameObject.SendMessage("TakeDamage", Stats.Dmg);
            
        }

        SoundManager.instance.PlaySingle(scratchSfx);
        
    }

    private void MovePlayer(int xDir, int yDir)
    {
        // Change the walking animation direction depending on where we're turning. 
        // Currently indivudally setting and resetting every time to prevent a delay. 
        if (xDir < 0)
        {
            anim.SetTrigger("turnLeft");
            anim.ResetTrigger("turnRight");
            anim.ResetTrigger("turnUp");
            anim.ResetTrigger("turnDown");
            Stats.Facing = Stats.Direction.LEFT;
        }
        else if (xDir > 0)
        {
            anim.SetTrigger("turnRight");
            anim.ResetTrigger("turnLeft");
            anim.ResetTrigger("turnUp");
            anim.ResetTrigger("turnDown");
            Stats.Facing = Stats.Direction.RIGHT;
        }
        else if (yDir > 0)
        {
            anim.SetTrigger("turnUp");
            anim.ResetTrigger("turnRight");
            anim.ResetTrigger("turnLeft");
            anim.ResetTrigger("turnDown");
            Stats.Facing = Stats.Direction.UP;
        }
        else if (yDir < 0)
        {
            anim.SetTrigger("turnDown");
            anim.ResetTrigger("turnRight");
            anim.ResetTrigger("turnUp");
            anim.ResetTrigger("turnLeft");
            Stats.Facing = Stats.Direction.DOWN;
        }
        Vector2 startTile = transform.position;
        Vector2 targetTile = startTile + new Vector2(xDir, yDir);

        // Determine if the target tile is traversable.
        bool hasObstacle = getCell(obstaclesTilemap, targetTile) != null;
        bool hasWall = getCell(wallsTilemap, targetTile) != null;

        // Raycast to check if an enemy prefab is on this tile. Cast a line from the start point to the end point on the Units layer.
        //Disable the boxCollider so that linecast doesn't hit this object's own collider.
        //boxCollider.enabled = false;
        // Set target tile for enemy spaces.
        //Vector2 rangeTile = startTile + new Vector2(xDir * Stats.Range, yDir * Stats.Range);
        RaycastHit2D hit = Physics2D.Linecast(startTile, targetTile, unitsMask);
        //Re-enable boxCollider after linecast
        //boxCollider.enabled = true;

        // If trying to move onto the same tile as an enemy that isn't at 0 hp, take a bit of damage. 
        if (hit.transform != null && hit.transform.gameObject.tag == "Enemy" && !hit.transform.gameObject.GetComponent<Enemy>().IsDefeated())
        {
            Stats.TakeDamage(0.5f);
            StartCoroutine(IsHit());
            StartCoroutine(BlockedMovement(targetTile));

        }
        // If the tile to move to does not contain a wall or obstacle, it's a valid move. 
        else if (!hasObstacle && !hasWall)
        {
            StartCoroutine(SmoothMovement(targetTile));
        }
        else
        {
            StartCoroutine(BlockedMovement(targetTile));
        }

        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Magic")
        {
            StartCoroutine(IsHit());
        }

    }
    
    /* COROUTINES */
    // 'Flash' the sprite when attacked.
    public IEnumerator IsHit()
    {
        SoundManager.instance.PlaySingle(hurtSfx);
        for (int i = 0; i < 5; i++)
        {
            rend.enabled = true;
            yield return new WaitForSeconds(0.1f);
            rend.enabled = false;
            yield return new WaitForSeconds(0.1f);
        }
        rend.enabled = true;
    }


    // Smoothly move the player from one tile to the next. 
    private IEnumerator SmoothMovement(Vector3 end)
    {
        isMoving = true;

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
       
        // Coroutine has finished moving the player.
        isMoving = false;
        SoundManager.instance.RandomizeSfx(steps1Sfx, steps2Sfx);
    }

    // Moves the player in the intended direction, then 'bounces' back to where they were. 
    // Gives feedback for an invalid move. 
    private IEnumerator BlockedMovement(Vector3 end)
    {
        isMoving = true;

        Vector3 originalPos = transform.position;

        // Update the end position to be a little bit in front of the current position to 'bounce' back from. 
        end = transform.position + ((end - transform.position) / 3);

        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        float inverseMoveTime = (1 / (moveTime * 2));

        // Move forwards.
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPos = Vector3.MoveTowards(transform.position, end, inverseMoveTime * Time.deltaTime);
            transform.position = newPos;
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            yield return null;
        }

        SoundManager.instance.PlaySingle(blockedSfx);

        // Move back to the original position.
        sqrRemainingDistance = (transform.position - originalPos).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPos = Vector3.MoveTowards(transform.position, originalPos, inverseMoveTime * Time.deltaTime);
            transform.position = newPos;
            
            sqrRemainingDistance = (transform.position - originalPos).sqrMagnitude;

            
        }



        isMoving = false;
    }

    // Introduce a delay between actions.
    private IEnumerator ActionCooldown(float cooldown)
    {
        onCooldown = true;
        while (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
            yield return null;
        }
        onCooldown = false;
    }
    
    /* TILEMAP/MOVEMENT UTILS */
    // Determine if the given cell position is part of this tilemap or not.
    private TileBase getCell(Tilemap tilemap, Vector2 cellWorldPos)
    {
        return tilemap.GetTile(tilemap.WorldToCell(cellWorldPos));
    }
    
}
