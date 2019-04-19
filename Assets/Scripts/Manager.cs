// Manages the current overall state of the game: the current time, whether the game is over, and restarting. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    // The default amount of time the game lets you progress for. Will eventually be pickable. 
   // private float DEFAULT_TIME = 90f;
    public static Manager instance = null;

    // UI to display the game over.
    private static GameObject gameOverImage;
    private static Text gameOverText;
    public static GameObject restartButton;
    public static GameObject homeButton;
    public static Text timerText;
    private static Text scoreText;
    private static Text multiplierText;

    private float countdownTime = 4.0f;
    public Text countdownText;

    private static GameObject[] pauseObjects;

   
    // Keep a reference to the current timer running process to be able to stop and restart it between runs. 
    Coroutine timerCoroutine;

    // The current bonus for enemy kills in this room. 
    private int enemyKillBonus;

    void Awake()
    {
       
        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;
        }
            

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);


        Input.ResetInputAxes();

        gameOverImage = GameObject.Find("GameOver");
        gameOverText = GameObject.Find("GameOverText").GetComponent<Text>();
        timerText = GameObject.Find("TimerText").GetComponent<Text>();
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        multiplierText = GameObject.Find("ScoreMultiplier").GetComponent<Text>();
        restartButton = GameObject.Find("RestartButton");
        homeButton = GameObject.Find("HubButton");
        // Prevent the UI assignment being reset.
        restartButton.GetComponent<Button>().onClick.AddListener(instance.RestartGame);
        homeButton.GetComponent<Button>().onClick.AddListener(instance.ReturnHome);
        //gameOverImage.SetActive(false);
        //restartButton.SetActive(false);
        //homeButton.SetActive(false);
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        hidePaused();
        Stats.onScoreChangedCallback += UpdateScore;
        Stats.onMultiplierChangedCallback += UpdateMultiplier;


    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        Time.timeScale = 0;
        // Reset any stats carried over from anywhere else.
        Stats.Reset();
        // Refill the item pools. 
        ItemManager.instance.RefillPools();
        yield return StartCoroutine(Countdown(Time.realtimeSinceStartup));
    }

    IEnumerator Countdown(float start)
    {
        while (countdownTime > 0)
        {
            countdownTime -= 1.0f;
            countdownText.text = (countdownTime).ToString("0");
            yield return new WaitForSecondsRealtime(1);
        }
        Time.timeScale = 1;
        countdownText.text = "";
        SoundManager.instance.runSource.Play();
        timerCoroutine = StartCoroutine(levelTimer());
    }
   

    IEnumerator levelTimer()
    {
        float counter = Stats.ChosenTime;

        while (counter > 0)
        {
            counter -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(counter / 60F);
            int seconds = Mathf.FloorToInt(counter - minutes * 60);
            string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);


            timerText.text = niceTime;
            yield return null;
        }

        // When this coroutine finishes, the elapsed run time has passed and the game is over. 
        timerText.text = string.Format("{0:0}:{1:00}", 0, 0);
        GameOver(false);

    }

    public void ResetTimer(float newTime)
    {
        StopCoroutine(timerCoroutine);
       // levelTimeInSeconds = newTime;
        timerCoroutine = StartCoroutine(levelTimer());
    }

    public void UpdateScore()
    {
        scoreText.text = "Score: " + Stats.Score;
    }

    public void UpdateMultiplier()
    {
        if (Stats.HpMultiplier == 1.0f)
        {
            multiplierText.text = "";
        }
        else
        {
            multiplierText.text = "No damage! " + Stats.HpMultiplier + "x";
        }
    }
    

    public void RestartGame()
    {
        // Clear all player stats and items. 
        Stats.Reset();
        // Refill item pools.
        ItemManager.instance.RefillPools();
        // Unpause the game.
        Time.timeScale = 1;
        
        // Restart the scene.
        SceneManager.LoadScene("Runner");
        // Restart timer.
        ResetTimer(Stats.ChosenTime);
        // Restart the background music. 
        SoundManager.instance.runSource.Stop();
        SoundManager.instance.runSource.Play();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
                showPaused();
            }
            else if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                hidePaused();
            }
        }
    }

    public void showPaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
        }
    }

    //hides objects with ShowOnPause tag
    public void hidePaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
        }
    }

    public void ReturnHome()
    {
        Stats.Reset();
        Stats.LastPos = new Vector2(0.5f, 0.5f);
        StopCoroutine(timerCoroutine);
        Time.timeScale = 1;
        SoundManager.instance.runSource.Stop();
        SceneManager.LoadScene("Hub");
        SoundManager.instance.hubSource.Play();
        Destroy(gameObject);
    }

    // Called when time is up or when the player is out of health.
    public void GameOver(bool isPlayerDead)
    {
        // Freeze the scene. 
        Time.timeScale = 0;
        gameOverImage.SetActive(true);
        restartButton.SetActive(true);
        homeButton.SetActive(true);
        if (isPlayerDead)
        {
            gameOverText.text = "Maybe next time...\nYou cleared " + (Stats.RoomCount - 1) + " rooms\n" + "Final score: " + Stats.Score; ;
        }
        else
        {
            gameOverText.text = "That's far enough for now... good job!\nYou cleared " 
                                + (Stats.RoomCount - 1) + " rooms\n"
                                + "Final score: " + Stats.Score;
        }
        
    }

    public void CalculateEnemyBonus(int noOfEnemies)
    {
        enemyKillBonus = Mathf.FloorToInt(Mathf.Ceil(Mathf.Pow(noOfEnemies, 0.2f) * 15));
    }

    public int GetEnemyBonus()
    {
        return enemyKillBonus;
    }
}
