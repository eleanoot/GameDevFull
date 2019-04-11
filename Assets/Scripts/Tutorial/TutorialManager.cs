using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private static Camera cam;
    public static TutorialManager instance = null;
    public bool roomCompleted = false; 

    private void Awake()
    {
        // Check if instance already exists
        if (instance == null)
        {
            cam = FindObjectOfType<Camera>();
            //if not, set instance to this
            instance = this;
        }


        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    public void OnRoomClear()
    {
        cam.transform.Translate(new Vector3(0, 10, 0));
    }
    
}
