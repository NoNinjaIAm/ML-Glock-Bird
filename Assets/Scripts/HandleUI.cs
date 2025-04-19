using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class HandleUI : MonoBehaviour
{
    // Ui Variables
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject titleScreen;

    private float finalScore = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGameStart += HandleStartGame;
        if (scoreText == null || finalScoreText == null || gameOverScreen == null || titleScreen == null) Debug.LogError("UI ERROR: SOMETHING IS NULL");
    }
    private void HandleStartGame()
    {
        // Event handling
        GameManager.Instance.OnGameOver += HandleGameOver;
        GameManager.Instance.OnGameStart -= HandleStartGame;
        GameManager.Instance.OnScoreChanged += HandleScoreChange;

        // Disable Title
        titleScreen.SetActive(false);
        // Game Display
        scoreText.gameObject.SetActive(true);
        scoreText.text = 0.ToString(); // !! Assuming we always start game with score of 0
    }
    private void HandleGameOver()
    {
        // Event Handling
        GameManager.Instance.OnGameOver -= HandleGameOver;
        GameManager.Instance.OnScoreChanged -= HandleScoreChange;

        scoreText.gameObject.SetActive(false);
        // enable game over screen
        gameOverScreen.SetActive(true);
        finalScoreText.text = "Final Score: " + finalScore.ToString();
    }

    private void HandleScoreChange(int newScore)
    {
        finalScore = newScore;
        scoreText.text = newScore.ToString();
    }

    // Fail Safe
    private void OnDestroy()
    {
        GameManager.Instance.OnGameStart -= HandleStartGame;
        GameManager.Instance.OnGameOver -= HandleGameOver;
        GameManager.Instance.OnScoreChanged -= HandleScoreChange;
    }
}
