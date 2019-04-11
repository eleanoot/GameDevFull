// Manages continous playing of the background music and eventually sound effects. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource bgSource;
    public static SoundManager instance = null;
    void Awake()
    {
        // Check if there is already an instance of a SoundManager.
        if (instance == null)
            instance = this;
        // Else if instance already exists, destroy it.
        else if (instance != this)
            Destroy(gameObject);

        // Don't destroy so the background music doesn't restart.
        DontDestroyOnLoad(gameObject);
    }
    
}
