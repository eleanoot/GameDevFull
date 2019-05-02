using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class IntroController : MonoBehaviour
{
    [SerializeField]
    Image[] images;
    [SerializeField]
    Text endingText;

    private void Awake()
    {
        foreach (Image i in images)
        {
            i.canvasRenderer.SetAlpha(0.0f);
        }
        endingText.canvasRenderer.SetAlpha(0.0f);
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return StartCoroutine(DoSlowFadeIn(0));
        yield return StartCoroutine(DoSlowFadeIn(1));
        yield return StartCoroutine(DoSlowFadeIn(2));

        // Fade out first 3 images.
        images[0].CrossFadeAlpha(0.0f, 1.5f, false);
        images[1].CrossFadeAlpha(0.0f, 1.5f, false);
        images[2].CrossFadeAlpha(0.0f, 1.5f, false);

        yield return new WaitForSeconds(2);

        yield return StartCoroutine(DoFastFadeIn(3));
        yield return StartCoroutine(DoFastFadeIn(4));
        yield return StartCoroutine(DoFastFadeIn(5));

        yield return new WaitForSeconds(2.5f);

        images[3].CrossFadeAlpha(0.0f, 1.5f, false);
        images[4].CrossFadeAlpha(0.0f, 1.5f, false);
        images[5].CrossFadeAlpha(0.0f, 1.5f, false);

        yield return new WaitForSeconds(2);

        yield return StartCoroutine(DoSlowFadeIn(6));

        yield return new WaitForSeconds(3);

        images[6].CrossFadeAlpha(0.0f, 1.5f, false);

        yield return new WaitForSeconds(2);

        endingText.CrossFadeAlpha(1.0f, 1.0f, false);

        yield return new WaitForSeconds(6);

        endingText.CrossFadeAlpha(0.0f, 1.0f, false);

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene("Title");
    }

    IEnumerator DoSlowFadeIn(int imageNo)
    {
        images[imageNo].CrossFadeAlpha(1.0f, 1.5f, false);
        yield return new WaitForSeconds(3);
    }

    IEnumerator DoFastFadeIn(int imageNo)
    {
        images[imageNo].CrossFadeAlpha(1.0f, 1.0f, false);
        yield return new WaitForSeconds(1);
    }






    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Title");
        }
    }
}
