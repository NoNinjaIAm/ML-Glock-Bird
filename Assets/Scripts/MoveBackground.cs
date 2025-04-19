using UnityEngine;

public class MoveBackground : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    private BoxCollider2D boxCollider;
    private Vector2 startPosition;
    private float repeatWidth;
    void Start()
    {
       startPosition = transform.position;
       boxCollider = GetComponent<BoxCollider2D>();
       repeatWidth = boxCollider.size.x / 2f;
       GameManager.Instance.OnGameOver += OnGameOver;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < startPosition.x - repeatWidth)
        {
            transform.position = startPosition;
        }
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void OnGameOver()
    {
        GameManager.Instance.OnGameOver -= OnGameOver;
        enabled = false;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameOver -= OnGameOver;
    }
}
