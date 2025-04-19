using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject pipeSetPrefab;
    [SerializeField] private GameObject gunPrefab;

    [SerializeField] private float gunSpawnProb = 0.5f;
    private float spawnPipePositionX = 11.0f;
    private float spawnGunPositionX = 11.0f;
    private Vector2 spawnPipeRangeY = new Vector2(-3f, 3f);
    private Vector2 spawnGunRangeY = new Vector2(-4.5f, 4.5f);


    [SerializeField] private float startRepeatTime = 1.0f;
    private float repeatTime;
    private float repeatScale = 0.1f;
    private float minRepeatTime = 2.5f;

    bool spawning = false;
    private float cumScore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        repeatTime = startRepeatTime;
        GameManager.Instance.OnGameStart += HandleStartGame;
    }

    private void HandleStartGame()
    {
        cumScore = 0.0f;
        spawning = true;
        StartCoroutine(SpawnObjects());

        // Stop listening for game start
        GameManager.Instance.OnGameStart -= HandleStartGame;
        // Start listening for game over
        GameManager.Instance.OnGameOver += HandleGameOver;
        // Start Listening for changes in score
        GameManager.Instance.OnScoreChanged += HandleScoreChange;
    }

    private void HandleScoreChange(int newScore)
    {
        cumScore = newScore;
    }

    private void HandleGameOver()
    {
        spawning = false;

        // Shut down events
        GameManager.Instance.OnGameOver -= HandleGameOver;
        GameManager.Instance.OnScoreChanged -= HandleScoreChange;

        StopAllCoroutines(); // Stops all running coroutines for safety
    }

    private IEnumerator SpawnObjects ()
    {
        while(spawning)
        {
            yield return new WaitForSeconds(repeatTime);

            // Generate random number between 0.01-1
            float randomNumber = Random.Range(0.01f, 1.00f);

            GameObject objectToSpawn;
            Vector2 spawnRangeY;
            float spawnPositionX;

            // If we spawn a gun
            if (randomNumber <= gunSpawnProb)
            {
                objectToSpawn = gunPrefab;
                spawnRangeY = spawnGunRangeY;
                spawnPositionX = spawnGunPositionX;
            }
            else // Pipe is default case
            {
                objectToSpawn = pipeSetPrefab;
                spawnRangeY = spawnPipeRangeY;
                spawnPositionX = spawnPipePositionX;
            }
            float spawnPositionY = Random.Range(spawnRangeY.x, spawnRangeY.y);

            Instantiate(objectToSpawn, new Vector2(spawnPositionX, spawnPositionY), objectToSpawn.transform.rotation);

            // Calculate new repeat time
            if(repeatTime > minRepeatTime)
            {
                repeatTime = startRepeatTime - (cumScore * repeatScale);
            }
            else
            {
                repeatTime = minRepeatTime;
            }
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStart -= HandleStartGame;
        GameManager.Instance.OnGameOver -= HandleGameOver;
        GameManager.Instance.OnScoreChanged -= HandleScoreChange;
    }
}
