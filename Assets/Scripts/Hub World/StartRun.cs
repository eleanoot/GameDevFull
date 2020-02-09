// General purpose script for triggering starting a run of a set amount of seconds. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartRun : MonoBehaviour
{
    // How long the run can last in seconds. 
    [SerializeField]
    private float runTime;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Stats.ChosenTime = runTime;
        Stats.Reset();
        SoundManager.instance.hubSource.Stop();
        SceneManager.LoadScene("Runner");
    }
}
