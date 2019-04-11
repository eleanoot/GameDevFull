// Script for when the player reaches the top of the room and should move on. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitRoom : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Don't activate scene change by an enemy attack!
        if (collision.gameObject.tag == "Player")
        {
            // If any items are active on the board, disable them.
            if (Stats.active != null)
                Stats.active.gameObject.SetActive(false);
            // Add the room cleared score. 
            Stats.AddScore(50 * (Stats.ItemRoomCount+1));
            // Add a score bonus based on the amount of health the player has on room clear.
            Stats.AddScore((int)(Stats.Hp * 30));
            if (Stats.PreviousHp == Stats.Hp)
                Stats.IncrementMultiplier(0.5f);
            else
            {
                // Only reset the multipler outside of leaving item rooms in the case of a HP up item being chosen.
                if (Stats.RoomCount % 5 != 0 && Stats.ItemRoomCount != 0)
                    Stats.IncrementMultiplier(0);
            }
                
            Stats.PreviousHp = Stats.Hp;
            // Reload the room to generate a new one. 
            SceneManager.LoadScene("Runner");
        }
    }
}
