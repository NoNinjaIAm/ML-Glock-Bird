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


}
