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
        gameOverImage.SetActive(false);
        restartButton.SetActive(false);
        homeButton.SetActive(false);

        Stats.onScoreChangedCallback += UpdateScore;
        Stats.onMultiplierChangedCallback += UpdateMultiplier;


    }

    // Start is called before the first frame update
    void Start()
    {
        // Reset any stats carried over from anywhere else.
        Stats.Reset();
        // Refill the item pools. 
        ItemManager.instance.RefillPools();
        SoundManager.instance.bgSource.Play();
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
        SoundManager.instance.bgSource.Stop();
        SoundManager.instance.bgSource.Play();

    }

    public void ReturnHome()
    {
        Stats.Reset();
        StopCoroutine(timerCoroutine);
        Time.timeScale = 1;
        SoundManager.instance.bgSource.Stop();
        SceneManager.LoadScene("Hub");
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
