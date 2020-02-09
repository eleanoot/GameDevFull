// Show the title screen text and start the game. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class TitleScreen : MonoBehaviour
{
    public AudioClip startSfx;
    public CanvasGroup canvas;
    public Image bg;

    private void Awake()
    {
        // Set everything to transparent to fade it in. 
        canvas.alpha = 0.0f;
    }

    private void Start()
    {
        SoundManager.instance.hubSource.Play();
        StartCoroutine(FadeIn());
    }

    // Update is called once per frame
    void Update()
    {
        // On any key pressed, start the game.
        if (Input.anyKey)
        {
            SoundManager.instance.PlaySingle(startSfx);
            bg.CrossFadeAlpha(0, 1.0f, false);
            StartCoroutine(DoFade());
            
        }
    }

    IEnumerator DoFade()
    {
        while (canvas.alpha > 0)
        {
            canvas.alpha -= Time.deltaTime / 3;
            
            yield return null;
        }
        canvas.interactable = false;
        SceneManager.LoadScene("Hub");
        yield return null;
    }

    IEnumerator FadeIn()
    {
        while (canvas.alpha < 1)
        {
            canvas.alpha += Time.deltaTime / 3;

            yield return null;
        }
        yield return null;
    }
}
