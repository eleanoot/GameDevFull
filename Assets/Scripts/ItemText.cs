// Object to behave as a trigger tile for stepping close to an item and displaying its description. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemText : MonoBehaviour
{

    private Item item;

    // Turn on and off text display when the player is on the trigger.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        item.SendMessage("DisplayText", true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        item.SendMessage("DisplayText", false);
    }

    // Start is called before the first frame update
    void Start()
    {
        item = GetComponentInParent<Item>();
    }
    
}
