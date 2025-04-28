using UnityEngine;

public class DestroyOutOfBounds : MonoBehaviour
{
    private float xBound = 20f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xBound = transform.parent.localPosition.x + 30f;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.x > xBound || transform.position.x < -xBound)
        {
            Destroy(gameObject);
        }
    }
}
