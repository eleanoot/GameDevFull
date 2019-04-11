// A minimal version of the RandomNumberGenerator, set to exactly the same values for tutorial purposes. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRNG : MonoBehaviour
{

    // Two seperate lists: one for dealing with item rarity, the other for everything else. 
    // A seperate list for item rarity was simpler for dealing with the relative probablity. 
    private static List<int> randomNumbers;
    private static List<int> itemRarity;

    // Mark where the game currently is in using both lists.
    private static int currentRandomIndex;
    private static int currentItemIndex;

    [SerializeField]
    private string seed = "TUTORIAL";

    // Start is called before the first frame update
    void Awake()
    {
       // Seeds based on entering words and converting them to a hash. 
       Random.InitState(seed.GetHashCode());
            

       randomNumbers = new List<int>();

       for (int i = 0; i < 5000; i++)
       {
         randomNumbers.Add(Random.Range(11, 78));
       }

       itemRarity = new List<int>();

       for (int i = 0; i < 5000; i++)
       {
         itemRarity.Add(Random.Range(0, 12)); // 8 = common, 3 = rare, 1 = legendary. 
       }

        currentRandomIndex = 0;
        currentItemIndex = 0;
    }


    // Retrieve the next random number.
    public int Next()
    {
        int nextNo = randomNumbers[currentRandomIndex];
        currentRandomIndex++;

        // If somehow reached the end of all the random numbers generated, roll some more. 
        if (currentRandomIndex == randomNumbers.Count - 1)
        {
            currentRandomIndex = 0;
            randomNumbers.Clear();
            for (int i = 0; i < 5000; i++)
            {

                randomNumbers.Add(Random.Range(11, 78));

            }
        }

        return nextNo;
    }

    // Retrieve the next item probabilty. 
    public int NextItem()
    {
        int nextNo = itemRarity[currentItemIndex];
        currentItemIndex++;

        // If somehow reached the end of all the random numbers generated, roll some more. 
        if (currentItemIndex == itemRarity.Count - 1)
        {
            currentItemIndex = 0;
            itemRarity.Clear();
            for (int i = 0; i < 5000; i++)
            {
                itemRarity.Add(Random.Range(0, 12)); // 8 = common, 3 = rare, 1 = legendary. 
            }
        }

        return nextNo;
    }


}
