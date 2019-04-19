// Base point for the overall generation of any normal or item room. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class RoomGeneration : MonoBehaviour
{
    // The possible obstacle tileset.
    [SerializeField]
    private TileBase[] obstacleTiles;
    // The possible sizes of regions the obstacles will be laid out in, e.g. 2x1
    [SerializeField]
    private Vector2Int[] possibleObstacleSizes;
    [SerializeField]
    private int noOfObstacles;

    // The possible enemies to spawn. Can eventually be split up into variants of different enemies to spawn at higher levels. 
    [SerializeField]
    private GameObject[] possibleEnemies;
    
    // Parameters for the floor decorations and how they could be laid out. Similar to obstacles. 
    [SerializeField]
    private TileBase[] decorationTiles;
    [SerializeField]
    private Vector2Int[] possibleDecorationSizes;
    [SerializeField]
    private int noOfDecorations;

    // The references for tilemaps that will be dynamically added to in room generation.
    private Tilemap obstacleTilemap;
    private Tilemap decorationTilemap;

    // Hold the current room we're generating. 
    private Room currentRoom;
    
    // The relevant UI element for displaying which room the player is currently in. 
    private Text roomText;
    

    // Start is called before the first frame update
    void Start()
    {
        this.currentRoom = new Room();
        Grid roomObject = GetComponent<Grid>();
        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();
        obstacleTilemap = tilemaps[2];

        // Increment the room count and display it in the GUI.
        roomText = GameObject.Find("RoomText").GetComponent<Text>();
        Stats.RoomCount++;
        roomText.text = "Room " + Stats.RoomCount;

        // Keep the previous values from room clear displayed. 
        Manager.instance.UpdateMultiplier();
        Manager.instance.UpdateScore();

        // Increment active charge count from passing into another room.
        Stats.CurrentCharge++;

        // Normal room, so place obstacles and enemies. 
        if (Stats.RoomCount % 5 != 0)
        {
            this.currentRoom.PopulateObstacles(this.noOfObstacles, this.possibleObstacleSizes);
            this.currentRoom.PopulateEnemies(this.possibleEnemies);
            this.currentRoom.AddPopulationToTilemap(obstacleTilemap, this.obstacleTiles);
        }
        // Item room. No enemies, just three items randomly rolled to choose from in the same positions every time. 
        // Spawn an item room every 5 rooms (may change if too frequent).
        else
        {
            GameObject[] chosenItems = new GameObject[3];
            for (int i = 0; i < 3; ++i)
            {
                Item nextItem;
                // Continue to roll this item until it is different from all the others rolled so far.
                do
                {
                    nextItem = ItemManager.instance.RollItem();
                } while (chosenItems[0] == nextItem.gameObject || chosenItems[1] == nextItem.gameObject);
                chosenItems[i] = nextItem.gameObject;
            }
            this.currentRoom.PopulateItems(chosenItems);
            this.currentRoom.AddPopulationToTilemap(obstacleTilemap, this.obstacleTiles);

            Stats.ItemRoomCount++;
        }
        
        decorationTilemap = tilemaps[3];
        this.currentRoom.PopulateDecorations(this.noOfDecorations, this.possibleDecorationSizes);
        this.currentRoom.AddDecorationToTilemap(decorationTilemap, this.decorationTiles);
    }

    
    
}
