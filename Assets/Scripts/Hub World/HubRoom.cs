using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubRoom : MonoBehaviour
{
    public GameObject virtualCamera;
    public GameObject ui;
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ui.SetActive(true);
            virtualCamera.SetActive(true);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ui.SetActive(false);
            virtualCamera.SetActive(false);
        }
    }
}
