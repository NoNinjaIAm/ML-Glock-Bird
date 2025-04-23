using UnityEngine;

public class PipeHandler : MonoBehaviour
{
    public SpawnManager spawnManager; // Passed into this upon creation
    private float xBound = 20f;
    public GameObject pipeGap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnManager.OnDestroyingAllPipes += HandleDestruction;
        spawnManager.RegisterPipeGap(pipeGap.transform);
    }

    void Update()
    {
        if (transform.position.x > xBound || transform.position.x < -xBound)
        {
            Destroy(gameObject);
        }
    }


    void HandleDestruction()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        spawnManager.UnregisterPipeGap(pipeGap.transform);
        spawnManager.OnDestroyingAllPipes -= HandleDestruction;
    }

}
