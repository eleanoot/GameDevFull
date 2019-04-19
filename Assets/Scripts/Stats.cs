using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Static script to maintain player stats and modifiers between scenes
// HP methods adapted from the free Unity store asset 'Simple Health Heart System' by ariel oliveira [o.arielg@gmail.com]

public class Stats
{
    public enum Direction
    {
        LEFT,
        RIGHT,
        UP,
        DOWN,
    }

    private static Direction facing;

    private static float chosenTime;
    // Used to match up the 
    private static Vector2 lastPos = new Vector2(0.5f, -4.5f);

    // Count the number of ALL rooms the player's passed through.
    private static int roomCount = 0;
    // Count the number of ITEM rooms the player's passed through. Used for enemy number scaling. 
    private static int itemRoomCount = 0;
    // the total possible number of heart containers the player can have
    public const int MAX_TOTAL_HEALTH = 6;
    public delegate void OnHealthChangedDelegate();
    public static OnHealthChangedDelegate onHealthChangedCallback;
    public static bool isInvulnerable = false; // prevent hits for the period of sprite flash. 

    // the player's current hp 
    private static float hp = 3.0f;
    // the total amount of hp the player can currently have 
    private static float maxHp = 3.0f;
    // used for the no health lost score bonus. 
    private static float previousHp = 3.0f;
    private static float hpMultiplier = 1.0f;
    public delegate void OnMultiplierChangedDelegate();
    public static OnMultiplierChangedDelegate onMultiplierChangedCallback;

    // the amount of damage the player's melee attack currently does.
    private static float dmg = 0.5f;
    // the number of tiles away the player can melee attack. 
    private static int range = 1;

    // the cooldown required between movements.
    private static float speed = 0.3f;

    // Each type of item the player has. 
    public static List<Item> passives = new List<Item>();
    public static Item active;

    private static bool shield = false;

    // Active item charge. Stored in here rather than in item to retain between rooms. 
    // The amount of charge this item needs to be used. 
    private static int activeItemCharge;
    // The current charge on the item.
    private static int currentCharge;

    private static int score = 0;
    public delegate void OnScoreChangedDelegate();
    public static OnScoreChangedDelegate onScoreChangedCallback;


    /* RESET TO DEFAULTS */
    public static void Reset()
    {
        hp = 3.0f;
        maxHp = 3.0f;
        previousHp = 3.0f;
        hpMultiplier = 1.0f;
        dmg = 0.5f;
        range = 1;
        speed = 0.3f;
        score = 0;
        passives.Clear();
        // Remove active items kept for use and any on the board.
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

        foreach (GameObject i in items)
        {
            GameObject.Destroy(i);
        }
        active = null;
        roomCount = 0;
        itemRoomCount = 0;
        activeItemCharge = 0;
        currentCharge = 0;

        lastPos = new Vector2(0.5f, -4.5f);
    }

    public static Vector2 LastPos
    {
        get
        {
            return lastPos;
        }
        set
        {
            lastPos = value;
        }
    }

    public static Direction Facing
    {
        get
        { return facing;  }
        set
        {
            facing = value;
        }
    }

    /* RUN TIME */
    public static float ChosenTime
    {
        get
        {
            return chosenTime;
        }
        set
        {
            chosenTime = value;
        }
    }

    /* SCORE */
    public static int Score
    {
        get
        {
            return score;
        }
    }

    public static void AddScore(int value)
    {
        score += value;
        if (onScoreChangedCallback != null)
            onScoreChangedCallback.Invoke();
    }

    /* ROOM COUNT */
    public static int RoomCount
    {
        get
        {
            return roomCount;
        }
        set
        {
            roomCount = value;
        }
    }

    public static int ItemRoomCount
    {
        get
        {
           return itemRoomCount;
        }
        set
        {
            itemRoomCount = value; 
        }
    }

    /* HP */
    public static float Hp
    {
        get
        {
            return hp;
        }
        set 
        {
            hp = value;
        }
    }

    public static float MaxHp
    {
        get
        {
            return maxHp;
        }
    }

    public static float PreviousHp
    {
        get
        {
            return previousHp;
        }
        set
        {
            previousHp = value;
        }
    }

    public static float HpMultiplier
    {
        get
        {
            return hpMultiplier;
        }
    }

    public static bool Shield
    {
        get
        {
            return shield;
        }
        set
        {
            shield = value;
        }
    }

    public static void IncrementMultiplier(float value)
    {
        if (value == 0)
            hpMultiplier = 1.0f;
        else
            hpMultiplier += value;

        if (onMultiplierChangedCallback != null)
            onMultiplierChangedCallback.Invoke();
    }

    public static void Heal(float health)
    {
            hp += health;
            ClampHealth();
        
    }
    public static bool TakeDamage(float dmg)
    {
        if (!isInvulnerable)
        {
            hp -= dmg;
            ClampHealth();
            IncrementMultiplier(0);
            if (hp <= 0 && SceneManager.GetActiveScene().name != "Tutorial")
            {
                Manager.instance.GameOver(true);
            }
            return true;
        }
        else
        {
            return false;
        }
       
    }

    public void AddHealth()
    {
        if (maxHp < MAX_TOTAL_HEALTH)
        {
            maxHp += 1;
            hp = maxHp;

            if (onHealthChangedCallback != null)
                onHealthChangedCallback.Invoke();
        }
    }

    static void ClampHealth()
    {
        hp = Mathf.Clamp(hp, 0, maxHp);

        if (onHealthChangedCallback != null)
            onHealthChangedCallback.Invoke();
    }

    /* DAMAGE */
    public static float Dmg
    {
        get
        {
            return dmg;
        }
        set
        {
            dmg = value;
        }
    }

    /* SPEED */
    public static float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            speed = value;
        }
    }

    /* RANGE */
    public static int Range
    {
        get
        {
            return range;
        }
        set
        {
            range = value;
        }
    }

    /* ACTIVE ITEM */
    public static int ActiveCharge
    {
        get
        {
            return activeItemCharge;
        }
        set
        {
            activeItemCharge = value;
        }
    }

    public static int CurrentCharge
    {
        get
        {
            return currentCharge;
        }
        set
        {
            currentCharge = value;
            // Prevent charge overflow.
            if (currentCharge > activeItemCharge)
                currentCharge = activeItemCharge;

            // Update the UI bar. Scale the amount the bar is filled based on how many charges the item requires. 
            GameObject.Find("ChargeBar").GetComponent<ProgressBar>().BarValue = currentCharge * (100 / (Stats.ActiveCharge == 0 ? 100 : Stats.ActiveCharge));
        }
    }

    public static Item ActiveItem
    {
        set
        {
            active = value;
            GameObject.Find("ActiveItem").GetComponent<ItemUI>().UpdateItem();
        }
        get
        {
            return active;
        }
    }

    // Convert the transform of a position into a tile space on the grid. Currently in here for accesibility everywhere. 
    public static Vector2Int TransformToGrid(Vector2 t)
    {
        return new Vector2Int((int)(t.x - 0.5f + 4), (int)(t.y - 0.5f + 4));
    }

}
