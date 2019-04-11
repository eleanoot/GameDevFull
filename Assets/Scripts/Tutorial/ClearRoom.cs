using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearRoom : MonoBehaviour
{ 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Reset everything from the tutorial before the return to the hub. 
        Stats.Reset();
        SceneManager.LoadScene("Hub");
    }
}
