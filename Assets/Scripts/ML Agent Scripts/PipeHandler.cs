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
        xBound = transform.parent.localPosition.x + 30f;
    }

    void Update()
    {
        if (transform.localPosition.x > xBound || transform.localPosition.x < -xBound)
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
