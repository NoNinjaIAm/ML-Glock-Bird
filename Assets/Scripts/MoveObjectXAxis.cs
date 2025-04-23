using JetBrains.Annotations;
using UnityEngine;

public class MoveObjectXAxis : MonoBehaviour
{
    [SerializeField] private bool movingRight = false;
    [SerializeField] private float speed = 1f;

    // Acceleration variables
    [SerializeField] private bool fasterWithProggresion = true;
    [SerializeField] private float xSpeedBound = 6f;
    
    void Start()
    {
        if (speed < 0) Debug.LogError("No Object Should have Negative Speed");
    }


    // Update is called once per frame
    void Update()
    {
        if (movingRight) TranslateXPerSecond(speed);
        else TranslateXPerSecond(-speed);
    }

    // Translate x function with respect to time
    private void TranslateXPerSecond(float velocity)
    {
        transform.Translate(new Vector2(velocity * Time.deltaTime, 0));
    }

}
