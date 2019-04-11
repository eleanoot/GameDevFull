// Represent the current active item and its charge in the UI for the player to see what they have an when it's available for use. 
// Could possibly also maintain a display of the passive items.
// Adapted from the free Unity asset 'Progress Bar' by UPLN.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    private Image activeImage;

    private ProgressBar chargeBar;

    // Start is called before the first frame update
    void Awake()
    {
        activeImage = GetComponent<Image>();
        chargeBar = GetComponentInChildren<ProgressBar>();
        UpdateItem();
    }

    public void UpdateItem()
    {
        if (Stats.ActiveItem != null)
        {
            activeImage.color = Color.white;
            activeImage.sprite = Stats.ActiveItem.GetComponent<SpriteRenderer>().sprite;
        }
        else // If we don't have an item, make the panel clear. 
        {
            activeImage.color = Color.clear;
        }
    }
    
}
