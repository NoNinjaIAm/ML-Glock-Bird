using Unity.VisualScripting;
using UnityEngine;

public class GunHandler : MonoBehaviour
{
    public SpawnManager spawnManager;
    private float xBound = 20f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnManager.OnDestroyingAllPipes += HandleGunDestruction;
        xBound = transform.parent.localPosition.x + 20f;

        spawnManager.RegisterPipeGap(transform); // Counting the gun as a pipe gap so bird goes for it
    }

    void HandleGunDestruction()
    {
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        spawnManager.OnDestroyingAllPipes -= HandleGunDestruction;
        spawnManager.UnregisterPipeGap(transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localPosition.x > xBound || transform.localPosition.x < -xBound)
        {
            Destroy(gameObject);
        }
    }
}
