// Manages the item pools and deals with rolling items for item rooms. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    // Prevent the actual item pools from being refilled each time a room is reloaded. 
    public static ItemManager instance = null;

    public GameObject[] allItems; // the actual item prefabs to be instantiated.

    public Item defaultItem;

    // Manage UI elements for all items in one central place.
    public Text flavourText;

    // Item pools to split the items by rarity. Can possibly be arrays once the number of items in the game is more set. 
    [SerializeField]
    private static List<Item> commonPool = new List<Item>();

    [SerializeField]
    private static List<Item> rarePool = new List<Item>();

    [SerializeField]
    private static List<Item> legendaryPool = new List<Item>();

    // The relative probabilties of items of each type appearing in an item room. 
    private int commonProb = 8;
    private int rareProb = 3;
    private int legendProb = 1;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Go through all the items and fill the rarity item pools with them. 
            foreach (GameObject i in allItems)
            {
                Item item = i.GetComponent<Item>();
                switch (item.Rarity)
                {
                    case ((Item.ItemRarity)0):
                        commonPool.Add(item);
                        break;
                    case ((Item.ItemRarity)1):
                        rarePool.Add(item);
                        break;
                    case ((Item.ItemRarity)2):
                        legendaryPool.Add(item);
                        break;
                    default:
                        break; // shouldn't get here, but just in case before we try to remove something from a pool it isnt in.
                }

            }

            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        
    }

    public void RefillPools()
    {
        commonPool.Clear();
        rarePool.Clear();
        legendaryPool.Clear();
        foreach (GameObject i in allItems)
        {
            Item item = i.GetComponent<Item>();
            switch (item.Rarity)
            {
                case ((Item.ItemRarity)0):
                    commonPool.Add(item);
                    break;
                case ((Item.ItemRarity)1):
                    rarePool.Add(item);
                    break;
                case ((Item.ItemRarity)2):
                    legendaryPool.Add(item);
                    break;
                default:
                    break; // shouldn't get here, but just in case before we try to remove something from a pool it isnt in.
            }

        }
    }

    public void AddToPool(Item item)
    {
        switch (item.Rarity)
        {
            case ((Item.ItemRarity)0):
                commonPool.Add(item);
                break;
            case ((Item.ItemRarity)1):
                rarePool.Add(item);
                break;
            case ((Item.ItemRarity)2):
                legendaryPool.Add(item);
                break;
            default:
                break;
        }
    }

    public void RemoveFromPool(Item item)
    {
        int itemIndex;
        switch (item.Rarity)
        {
            case ((Item.ItemRarity)0):
                itemIndex = commonPool.FindIndex(x => x.ItemName() == item.ItemName());
                commonPool.RemoveAt(itemIndex);
                break;
            case ((Item.ItemRarity)1):
                itemIndex = rarePool.FindIndex(x => x.ItemName() == item.ItemName());
                rarePool.RemoveAt(itemIndex);
                break;
            case ((Item.ItemRarity)2):
                itemIndex = legendaryPool.FindIndex(x => x.ItemName() == item.ItemName());
                legendaryPool.RemoveAt(itemIndex);
                break;
            default:
                break; // shouldn't get here, but just in case before we try to remove something from a pool it isnt in.
        }

    }

    public Item RollItem()
    {
        // Determine the item rarity from the next random number. 
        int itemRarity = RandomNumberGenerator.instance.NextItem();
        itemRarity -= commonProb;
        Item.ItemRarity rarity;
        if (itemRarity < 0)
            rarity = Item.ItemRarity.Common;
        else
        {
            itemRarity -= rareProb;
            if (itemRarity < 0)
                rarity = Item.ItemRarity.Rare;
            else
                rarity = Item.ItemRarity.Legendary;
        }

        // Randomly choose an item from the respective pool.
        int nextItemIndex = RandomNumberGenerator.instance.Next();
        Item nextItem = null; 
        switch(rarity)
        {
            case Item.ItemRarity.Common:
                if (commonPool.Count == 0)
                    nextItem = defaultItem;
                else if (nextItemIndex >= commonPool.Count)
                {
                    nextItemIndex = ReduceNumber(nextItemIndex, commonPool.Count);
                    nextItem = commonPool[nextItemIndex];
                }
                    
                break;
            case Item.ItemRarity.Rare:
                if (rarePool.Count == 0)
                    nextItem = defaultItem;
                else if (nextItemIndex >= rarePool.Count)
                {
                    nextItemIndex = ReduceNumber(nextItemIndex, rarePool.Count);
                    nextItem = rarePool[nextItemIndex];
                }
                break;
            case Item.ItemRarity.Legendary:
                if (legendaryPool.Count == 0)
                    nextItem = defaultItem;
                else if (nextItemIndex >= legendaryPool.Count)
                {
                    nextItemIndex = ReduceNumber(nextItemIndex, legendaryPool.Count);
                    nextItem = legendaryPool[nextItemIndex];
                }
                    
                break;
        }

        return nextItem;
        
    }

    // Split the latest random number to be used into values that can be used by item generation. 
    // Split into single digits from the tens and ones and add together. If only one digit, simply subtract the maximum value. Repeat until number is small enough. 
    public int ReduceNumber(int num, int capacity)
    {
        int result = num;
        do
        {
            int rhs = result % 10; // ones
            int lhs = (result / 10) % 10; // tens

            result = lhs + rhs;

            if (result <= 10)
            {
                result = Mathf.Abs(result - capacity);
            }
        } while (result >= capacity);
        return result;
    }

   
}
