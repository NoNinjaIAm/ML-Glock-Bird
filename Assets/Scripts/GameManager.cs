using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private int score;
    private bool isGameOver = false;
    private bool onTitleScreen = true; // On title screen every time scene is loaded

    // Events
    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action<int> OnScoreChanged;

    private PlayerController playerController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            // Add listener for when new scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Variables to reset when a new scene is loaded
    private void ResetVariables()
    {
        // Reset Variables on scene reload
        score = 0;
        isGameOver = false;
        onTitleScreen = true;
    }

    // Listener for when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset variables when a scene is loaded
        ResetVariables();
        FindPlayerController(); // Gotta find PlayerController on every scene reload

        // Restart music
        SoundManager.instance.PlayMusic(); // Down the line this plays at the start of every scene not just game
    }

    private void FindPlayerController()
    {
        // Find the PlayerController in the new scene
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.PlayerPassedPipe += AddScore;
            playerController.PlayerDied += GameOver;
        }
        else
        {
            Debug.LogError("ERROR: Player Controller Not Found By GameManager!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if we're restarting (dumb ooga booga shit)
        if(isGameOver && Input.GetKeyUp(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if(onTitleScreen && Input.GetKeyUp(KeyCode.Space))
        {
            StartGame();
        }
    }

    private void AddScore()
    {
        score++;
        OnScoreChanged?.Invoke(score);
    }

    public int GetScore(){return score;}

    private void GameOver ()
    {
        isGameOver = true;

        // Handle Events
        playerController.PlayerPassedPipe -= AddScore;
        playerController.PlayerDied -= GameOver;

        // Sound Effects
        SoundManager.instance.StopMusic();
        SoundManager.instance.PlaySound("GameOver");

        // Game over event
        OnGameOver?.Invoke();
    }

    private void StartGame()
    {
        onTitleScreen = false;

        // Sound
        SoundManager.instance.PlaySound("UiSound");
        // StartGame Event
        OnGameStart?.Invoke();
    }

    // Clean up event listener when this object is destroyed (Failsafe)
    private void OnDestroy()
    {
        if(Instance == this)
        {
            playerController.PlayerPassedPipe -= AddScore;
            playerController.PlayerDied -= GameOver;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }


}
