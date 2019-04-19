using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartTutorial : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        SoundManager.instance.hubSource.Stop();
        SceneManager.LoadScene("Tutorial");
        SoundManager.instance.tutorialSource.Play();
    }
}
