using JetBrains.Annotations;
using UnityEngine;

public class MoveObjectXAxis : MonoBehaviour
{
    [SerializeField] private bool movingRight = false;
    [SerializeField] private float speed = 1f;
    
    // Acceleration variables
    private float accelFactor = 0.1f;
    [SerializeField] private bool fasterWithProggresion = true;
    [SerializeField] private float xSpeedBound = 6f;
    void Start()
    {
        if (speed < 0) Debug.LogError("No Object Should have Negative Speed");

        // Event Subscribe
        GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void HandleGameOver()
    {
        GameManager.Instance.OnGameOver -= HandleGameOver;
        enabled = false; // disable script when game ends
    }

    // Update is called once per frame
    void Update()
    {
        // Translate and scale with score increase
        if (fasterWithProggresion)
        {
            float velocityX = GetScaledVelocity();

            if (Mathf.Abs(velocityX) <= xSpeedBound) // If we're within max speed bound
            {
                TranslateXPerSecond(velocityX);
            }
            else // Reached max speed bound in some direction
            {
                if(movingRight) TranslateXPerSecond(xSpeedBound);
                else TranslateXPerSecond(-xSpeedBound);
            }
        }
        // Just translate normally
        else
        {
            if (movingRight) TranslateXPerSecond(speed);
            else TranslateXPerSecond(-speed);
        }
    }

    // Returned Velcoity scaled with time, if max velocity is reached then return max X velocity
    private float GetScaledVelocity()
    {
        float scaledSpeed = (speed + (GameManager.Instance.GetScore() * accelFactor));
        if (movingRight) return scaledSpeed;
        return -scaledSpeed;
    }

    // Translate x function with respect to time
    private void TranslateXPerSecond (float velocity)
    {
        transform.Translate(new Vector2(velocity * Time.deltaTime, 0));
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameOver -= HandleGameOver;
    }
}
