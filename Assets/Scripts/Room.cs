// Generation specific to the current room. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    // Represent the contents of the fillable grid with a 2D array of ID values to later instantiate. 
    private string[,] population;
    
    // Hold the names of the prefabs to instantiate later as a lookup. 
    private Dictionary<string, GameObject> name2Prefab;

    // The total number of obstacles and enemies on the board. 
    private int totalElementsOnBoard;
    // Represents the number of elements (obstacles, enemies, enemy attacks) on each of the main rows of the grid. A row will no longer be considered for addition to if 
    // 5 or more elements are already on it (the max that can cover the exit while still allowing a passage through).
    // index 0 = row 1, y coords go from 0 to 7 on the main grid. 
    private int[] elementsPerRow;

    // The max number of fillable grid rows on the grid before the grid is considered 'filled'.
    private const int MAX_ROWS_FILLED = 4;
    // The max number of fillable spaces along a row before a row is considered 'filled'. 
    private const int MAX_ROW_AMOUNT = 5;
    // The maximum number of enemies to generate in this room, no matter how many rows or spaces are filled. 
    private int enemyNumCap;

    private Vector2Int[] enemyPositions; 

    public Room()
    {
        // Initialise the grid.
        this.population = new string[10, 10];
        for (int xIndex = 0; xIndex < 10; xIndex++)
        {
            for (int yIndex = 0; yIndex < 10; yIndex++)
            {
                this.population[xIndex, yIndex] = "";
            }
        }

        this.name2Prefab = new Dictionary<string, GameObject>();
        totalElementsOnBoard = 0;
        elementsPerRow = new int[8];
        enemyPositions = new Vector2Int[64];
        for (int i = 0; i < enemyPositions.Length; i++)
        {
            enemyPositions[i] = new Vector2Int(-10,-10);
        }

        // Enemy number scaling based on a y = 3^x graph, where x is the number of item rooms passed through.
        // Will eventually take into account the chance to spawn a harder variant of an enemy when more are added. 
        // 0 and 1 item room(s) set manually due to the graph used giving them both the same very small value in this context.
        if (Stats.ItemRoomCount == 0)
        {
            enemyNumCap = 4;
        }
        else if (Stats.ItemRoomCount == 1)
            enemyNumCap = 6;
        else
        {
            enemyNumCap = (int)Mathf.Pow(3.0f, Stats.ItemRoomCount);
        }
        
    }

    // Add obstacle tiles to the tile positions marked by the grid.
    public void AddPopulationToTilemap(Tilemap tilemap, TileBase[] obstacleTiles)
    {
        for (int xIndex = 0; xIndex < 10; xIndex++)
        {
            for (int yIndex = 0; yIndex < 10; yIndex++)
            {
                if (this.population[xIndex, yIndex] == "Obstacle")
                {
                    // Choose a random tile from the obstacles to set here.
                    // Get the next random number to choose the size of this obstacle region.
                    int tileIndex = RandomNumberGenerator.instance.Next();
                    if (tileIndex >= obstacleTiles.Length)
                        tileIndex = ReduceNumber(tileIndex, obstacleTiles.Length);

                    TileBase obstacleTile = obstacleTiles[tileIndex];

                    // Need to subtract 4 as grid matrix (0,0) is lower left corner, but tilemap (0,0) is the centre of the 8x8 grid.
                    tilemap.SetTile(new Vector3Int(xIndex - 4, yIndex - 4, 0), obstacleTile);
                }
                else if (this.population[xIndex, yIndex] != "")
                {
                    GameObject prefab = GameObject.Instantiate(this.name2Prefab[this.population[xIndex, yIndex]]);
                    prefab.transform.position = new Vector2(xIndex - 4 + 0.5f, yIndex - 4 + 0.5f);

                    // If this is an item room, add the flavourtext tiles around this item. 
                    if (Stats.RoomCount % 5 == 0)
                    {
                        GameObject textPrefab = GameObject.Instantiate(Resources.Load("ItemText")) as GameObject;
                        // Translate this transform into a grid space, 1 away from the item. 
                        textPrefab.transform.position = new Vector2(xIndex - 1 - 4 + 0.5f, yIndex - 4 + 0.5f);
                        textPrefab.transform.SetParent(prefab.transform);

                        textPrefab = GameObject.Instantiate(Resources.Load("ItemText")) as GameObject;
                        textPrefab.transform.position = new Vector2(xIndex + 1 - 4 + 0.5f, yIndex - 4 + 0.5f);
                        textPrefab.transform.SetParent(prefab.transform);

                        textPrefab = GameObject.Instantiate(Resources.Load("ItemText")) as GameObject;
                        textPrefab.transform.position = new Vector2(xIndex - 4 + 0.5f, yIndex + 1 - 4 + 0.5f);
                        textPrefab.transform.SetParent(prefab.transform);

                        textPrefab = GameObject.Instantiate(Resources.Load("ItemText")) as GameObject;
                        textPrefab.transform.position = new Vector2(xIndex - 4 + 0.5f, yIndex - 1 - 4 + 0.5f);
                        textPrefab.transform.SetParent(prefab.transform);
                    }
                }
            }
        }
    }

    // Add decoration tiles to the tile positions marked by the grid.
    public void AddDecorationToTilemap(Tilemap tilemap, TileBase[] decorationTiles)
    {
        for (int xIndex = 0; xIndex < 10; xIndex++)
        {
            for (int yIndex = 0; yIndex < 10; yIndex++)
            {
                if (this.population[xIndex, yIndex] == "Decoration")
                {
                    // Choose a random tile from the obstacles to set here.
                    // Get the next random number to choose the size of this obstacle region.
                    int tileIndex = RandomNumberGenerator.instance.Next();
                    if (tileIndex >= decorationTiles.Length)
                        tileIndex = ReduceNumber(tileIndex, decorationTiles.Length);

                    TileBase decorationTile = decorationTiles[tileIndex];

                    // Need to subtract 4 as grid matrix (0,0) is lower left corner, but tilemap (0,0) is the centre.
                    tilemap.SetTile(new Vector3Int(xIndex - 4, yIndex - 4, 0), decorationTile);
                }
            }
        }
    }

    // Iterate through each chosen tile in the given region to check if they're all still available for placement. 
    private bool IsFree(List<Vector2Int> region)
    {
        foreach (Vector2Int tile in region)
        {
            if (this.population[tile.x, tile.y] != "")
            {
                return false;
            }
        }
        return true;
    }

    // Look for random areas on the grid until one is available to place tiles in. 
    private List<Vector2Int> FindFreeRegion(Vector2Int sizeInTiles, Enemy enemyDetails = null)
    {
        List<Vector2Int> region = new List<Vector2Int>();
        // Continue until a valid region for this tile set is found. 
        do
        {
            region.Clear();

            // Generate random centre for the tiles between the valid tiles in the full map (i.e. not the outside walls or entrance/exit rows)
            // Advance to the next random number in the seeded set for the x and y coord of this random centre. 
            // First digit = x, second digit = y.
            int coord = RandomNumberGenerator.instance.Next();
            int centreY = coord % 10; // ones
            if (centreY > 7)
                centreY = 7;
            int centreX = (coord / 10) % 10; // tens
            if (centreX > 7)
                centreX = 7;
            if (centreY < 1)
                centreY = 1; // Prevent an enemy spawning on the first row - unfair if directly in front of the player right away!

            if (enemyDetails != null)
            {
                if (enemyDetails.edgeEnemy)
                {
                    if (centreX < 5)
                        centreX = 0;
                    else
                        centreX = 7;
                }
            }

            Vector2Int centreTile = new Vector2Int(centreX, centreY);

            region.Add(centreTile);

            int initialXCoordinate = Mathf.Abs((centreTile.x - (int)Mathf.Floor(sizeInTiles.x / 2)));
            int initialYCoordinate = Mathf.Abs((centreTile.y - (int)Mathf.Floor(sizeInTiles.y / 2)));

            for (int xCoordinate = initialXCoordinate; xCoordinate < initialXCoordinate + sizeInTiles.x; xCoordinate++)
            {
                for (int yCoordinate = initialYCoordinate; yCoordinate < initialYCoordinate + sizeInTiles.y; yCoordinate++)
                {
                    region.Add(new Vector2Int(xCoordinate, yCoordinate));
                }
            }
        } while (!IsFree(region));
        return region;
    }

    // Choose a random size for this obstacle set and place it in the grid matrix.
    public void PopulateObstacles(int numberOfObstacles, Vector2Int[] possibleSizes)
    {
        for (int i = 0; i < numberOfObstacles; i++)
        {
            // Get the next random number to choose the size of this obstacle region.
            int sizeIndex = RandomNumberGenerator.instance.Next();
            if (sizeIndex >= possibleSizes.Length)
                sizeIndex = ReduceNumber(sizeIndex, possibleSizes.Length);

            Vector2Int regionSize = possibleSizes[sizeIndex];
            List<Vector2Int> region = FindFreeRegion(regionSize);
            foreach (Vector2Int coord in region)
            {
                if (this.population[coord.x, coord.y] == "")
                {
                    this.population[coord.x, coord.y] = "Obstacle";
                    // Update the number of elements on this row.
                    elementsPerRow[coord.y]++;
                }
               
            }
        }
    }

    // Choose a random size for this decoration set and place it in the grid matrix.
    public void PopulateDecorations(int numberOfDecorations, Vector2Int[] possibleSizes)
    {
        for (int i = 0; i < numberOfDecorations; i++)
        {
            // Get the next random number to choose the size of this obstacle region.
            int sizeIndex = RandomNumberGenerator.instance.Next();
            if (sizeIndex >= possibleSizes.Length)
                sizeIndex = ReduceNumber(sizeIndex, possibleSizes.Length);

            Vector2Int regionSize = possibleSizes[sizeIndex];
            List<Vector2Int> region = FindFreeRegion(regionSize);
            foreach (Vector2Int coord in region)
            {
                this.population[coord.x, coord.y] = "Decoration";
            }
        }
    }

    // Determine if this region is right next to the previously used region to try and space out elements. 
    //public bool CloseRegion(Vector2Int region, Vector2Int lastPos)
    //{
    //    return (region.x + 1 == lastPos.x || region.x - 1 == lastPos.x || region.y + 1 == lastPos.y || region.y - 1 == lastPos.y);
    //}

    public bool CloseRegion(Vector2Int region)
    {
        bool tooClose = false;
        foreach (Vector2Int pos in enemyPositions)
        {
            if (pos != new Vector2Int(-10, -10))
            {
                if (region.x + 1 == pos.x || region.x - 1 == pos.x || region.y + 1 == pos.y || region.y - 1 == pos.y)
                {
                    tooClose = true;
                    break;
                }
            }
        }

        return tooClose;
    }

    // Choose and place enemies until the grid is filled. 
    public void PopulateEnemies(GameObject[] possiblePrefabs)
    {
        int noOfEnemies = RandomNumberGenerator.instance.Next();

        // Reduce the given random number into one below the maximum number of enemies allowed for this room. 
        while (noOfEnemies > enemyNumCap)
        {
            noOfEnemies -= enemyNumCap;
        }
        if (noOfEnemies < 3)
        {
            noOfEnemies = 4; // A minimum number of enemies on the level.
        }
        int rowsFilled = 0;
        // Keep a reference to where the last enemy was placed to try and space them out. Initially set to a point nowhere near the grid for use in a loop.
        //Vector2Int lastEnemy = new Vector2Int(-10, -10);
        // Keep placing enemies until one of the stop conditions has been met for the grid being too full. 
        int count = 0;
        do
        {
            // Get the next random number to choose the enemy being added here.
            int choiceIndex = RandomNumberGenerator.instance.Next();
            if (choiceIndex >= possiblePrefabs.Length)
                choiceIndex = ReduceNumber(choiceIndex, possiblePrefabs.Length);

            GameObject prefab = possiblePrefabs[choiceIndex];

            Enemy enemyDetails = prefab.GetComponent<Enemy>();
            
            List<Vector2Int> region;
            // Repeat finding a position for this enemy until it's on a valid row and not close to the last enemy placed 
            // - attempt to spread them across the board. Try ten times before placing enemy in the last found position. 
            int attemptCount = 0;
            do
            {
                attemptCount++;
                region = FindFreeRegion(new Vector2Int(1, 1), enemyDetails);
            } while (elementsPerRow[region[0].y] == MAX_ROW_AMOUNT || CloseRegion(region[0]) && attemptCount < 10);

            if (this.population[region[0].x, region[0].y] == "")
            {
                this.population[region[0].x, region[0].y] = prefab.name;
                this.name2Prefab[prefab.name] = prefab;
                elementsPerRow[region[0].y]++;
                if (elementsPerRow[region[0].y] >= MAX_ROW_AMOUNT)
                    rowsFilled++;

                //lastEnemy = region[0];
                enemyPositions[count] = region[0];
            }

            // Add the tiles this enemy covers with their attack to the row counts- don't want the rows to become too filled with attacks that navigation isn't possible.
            Enemy chosenEnemy = prefab.GetComponent<Enemy>();
            Vector2Int[] targets = chosenEnemy.GetAttackTargets();
            foreach (Vector2Int coord in targets)
            {
                int newY = region[0].y + coord.y;
                if (newY >= 0 && newY < 8)
                {
                    elementsPerRow[newY]++;
                    if (elementsPerRow[newY] == MAX_ROW_AMOUNT)
                        rowsFilled++;
                }
                    
            }
            // Increment the number of enemies placed.
            count++;

        } while (rowsFilled <= MAX_ROWS_FILLED && count < noOfEnemies);

        Manager.instance.CalculateEnemyBonus(count);
    }

    // Place the chosen items in preset positions in the item room. 
    public void PopulateItems(GameObject[] items)
    {
        // Populated at preset positions for item rooms. 
        Vector2Int startPos = new Vector2Int(2, 4);
        
        for (int i = 0; i < items.Length; i++)
        {
            GameObject currentItem = items[i];
            this.population[startPos.x + i*2, startPos.y] = currentItem.name;
            this.name2Prefab[currentItem.name] = currentItem;
        }
    }

    // Split the random number into a value understandable by the grid placing methods. 
    // If the result is larger than the maximum value and double digits, add the two digits together and check again. 
    // If the result is larger than the maximum value and single digits/10 (as repeated addition of 1 and 0 causes a infinite loop), simply
    // subtract the maximum value. 
    public int ReduceNumber(int num, int max)
    {
        //int result = num;
        //do
        //{
        //    result %= max;
        //} while (result >= max);


        int result = num;
        do
        {
            int rhs = result % 10; // ones
            int lhs = (result / 10) % 10; // tens

            result = lhs + rhs;

            if (result <= 10)
            {
                //result = Mathf.Abs(result - max);
                result %= max;
            }
        } while (result >= max);

        return result;
    }
}
