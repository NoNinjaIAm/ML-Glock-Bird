using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

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

    bool spawning = true;
    private float cumScore = 0.0f;

    // ML Agent Stuff
    public List<Transform> pipeGaps = new List<Transform>();
    public event Action OnDestroyingAllPipes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void HandleStartEpisode()
    {
        StopAllCoroutines();
        OnDestroyingAllPipes?.Invoke(); // Kill all existing pipes

        repeatTime = startRepeatTime;
        StartCoroutine(SpawnObjects());
    }

    private void HandleScoreChange(int newScore)
    {
        cumScore = newScore;
        spawning = false;
    }


    private IEnumerator SpawnObjects ()
    {
        while(spawning)
        {
            yield return new WaitForSeconds(repeatTime);

            // Generate random number between 0.01-1
            float randomNumber = UnityEngine.Random.Range(0.01f, 1.00f);

            GameObject objectToSpawn;
            Vector2 spawnRangeY;
            float spawnPositionX;

            // If we spawn a gun
            if (randomNumber <= gunSpawnProb)
            {
                objectToSpawn = gunPrefab;
                spawnRangeY = spawnGunRangeY;
                spawnPositionX = spawnGunPositionX;
                float spawnPositionY = UnityEngine.Random.Range(spawnRangeY.x, spawnRangeY.y);

                // Pass reference to self to pipe
                var spawnedGun = Instantiate(objectToSpawn, new Vector2(spawnPositionX, spawnPositionY), objectToSpawn.transform.rotation);
                spawnedGun.GetComponent<GunHandler>().spawnManager = this;
            }
            else // Pipe is default case
            {
                objectToSpawn = pipeSetPrefab;
                spawnRangeY = spawnPipeRangeY;
                spawnPositionX = spawnPipePositionX;
                float spawnPositionY = UnityEngine.Random.Range(spawnRangeY.x, spawnRangeY.y);

                // Pass reference to self to pipe
                var spawnedPipe = Instantiate(objectToSpawn, new Vector2(spawnPositionX, spawnPositionY), objectToSpawn.transform.rotation);
                spawnedPipe.GetComponent<PipeHandler>().spawnManager = this;
            }
            

            

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

    public void RegisterPipeGap(Transform gap)
    {
        pipeGaps.Add(gap);
    }

    public void UnregisterPipeGap(Transform gap)
    {
        pipeGaps.Remove(gap);
    }


    private void OnDestroy()
    {
        return;
    }
}
