// Manages the current overall state of the game: the current time, whether the game is over, and restarting. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
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
    private static Text countdownText;

    // Objects that make up the pause menu to be shown on the pause screen.
    private static GameObject[] pauseObjects;

    private bool restarting;
   
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
        countdownText = GameObject.Find("CountdownText").GetComponent<Text>();
        timerText = GameObject.Find("TimerText").GetComponent<Text>();
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        multiplierText = GameObject.Find("ScoreMultiplier").GetComponent<Text>();
        restartButton = GameObject.Find("RestartButton");
        homeButton = GameObject.Find("HubButton");
        // Prevent the UI assignment being reset.
        restartButton.GetComponent<Button>().onClick.AddListener(instance.RestartGame);
        homeButton.GetComponent<Button>().onClick.AddListener(instance.ReturnHome);
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        hidePaused();
        Stats.onScoreChangedCallback += UpdateScore;
        Stats.onMultiplierChangedCallback += UpdateMultiplier;

        restarting = false;
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

    // Run the countdown before starting a run. 
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
        // Decrementing the timer. 
        if (counter > 0)
        {
            while (counter > 0)
            {
                counter -= Time.deltaTime;

                int minutes = Mathf.FloorToInt(counter / 60F);
                int seconds = Mathf.FloorToInt(counter - minutes * 60);
                string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);


                timerText.text = niceTime;
                yield return null;
            }
        }
        else
        {
            // Endless mode. Will never end by itself, and just keep adding onto the timer.
            while(true)
            {
                counter += Time.deltaTime;
                int minutes = Mathf.FloorToInt(counter / 60F);
                int seconds = Mathf.FloorToInt(counter - minutes * 60);
                string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);


                timerText.text = niceTime;
                yield return null;
            }
            
        }
        

        // When this coroutine finishes, the elapsed run time has passed and the game is over. 
        timerText.text = string.Format("{0:0}:{1:00}", 0, 0);
        GameOver(false);

    }

    public void ResetTimer(float newTime)
    {
        StopCoroutine(timerCoroutine);
        Stats.ChosenTime = newTime;
        timerCoroutine = StartCoroutine(levelTimer());
    }

    public void UpdateScore()
    {
        scoreText.text = "Score: " + Stats.Score;
    }

    // Update the multiplier for no damage taken.
    public void UpdateMultiplier()
    {
        // Multiplier lost. 
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
        SoundManager.instance.runSource.Stop();
        restarting = true;

    }

    IEnumerator DoCountdown()
    {
        countdownTime = 4.0f;
        Time.timeScale = 0;
        yield return StartCoroutine(Countdown(Time.realtimeSinceStartup));
        restarting = false;
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

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            GameObject[] enemies;
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject e in enemies)
            {
                e.gameObject.SendMessage("Defeat");
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

    // Perform necessary cleanup to return to the hub screen.
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
            gameOverText.text = "Maybe next time...\nYou cleared " + Mathf.Max(Stats.RoomCount - 1, 0) + " rooms\n" + "Final score: " + Stats.Score; ;
        }
        else
        {
            gameOverText.text = "That's far enough for now... good job!\nYou cleared " 
                                + (Stats.RoomCount - 1) + " rooms\n"
                                + "Final score: " + Stats.Score;
        }
        
    }

    // Calculate the bonus gotten from killing enemies in this current room. 
    public void CalculateEnemyBonus(int noOfEnemies)
    {
        enemyKillBonus = Mathf.FloorToInt(Mathf.Ceil(Mathf.Pow(noOfEnemies, 0.2f) * 15));
    }

    public int GetEnemyBonus()
    {
        return enemyKillBonus;
    }

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. 
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    // When this scene is reloaded, only start the countdown again if it's resetting to a totally new run.
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (restarting)
        {
            StartCoroutine(DoCountdown());
        }
    }
}
