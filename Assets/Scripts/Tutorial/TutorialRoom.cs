﻿// Script to activate the objects in this tutorial room when needed and transition between them. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRoom : MonoBehaviour
{
    public GameObject virtualCamera;
    public GameObject enemies;
    public GameObject ui;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // If any items are active on the board, disable them.
            if (Stats.active != null)
            {
                Stats.active.gameObject.SetActive(false);
                // Manually add one charge for the tutorial to get around its linear level setup. 
                Stats.CurrentCharge++;
            }
                
            // Only load the enemies in this room when it's entered to save on resources. 
            enemies.SetActive(true);
            ui.SetActive(true);
            virtualCamera.SetActive(true);
            Stats.RoomCount++;
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enemies.SetActive(false);
            // Clear any projectiles on the screen to prevent freezing them there. 
            Project[] projectiles = FindObjectsOfType<Project>();
            foreach (Project p in projectiles)
            {
                gameObject.transform.SetParent(null);
                gameObject.SetActive(false);
            }
            ui.SetActive(false);
            virtualCamera.SetActive(false);
            
        }
    }
}
