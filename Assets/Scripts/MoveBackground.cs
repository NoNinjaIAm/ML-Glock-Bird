using UnityEngine;

public class MoveBackground : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    private BoxCollider2D boxCollider;
    private Vector2 startPosition;
    private float repeatWidth;
    void Start()
    {
       startPosition = transform.localPosition;
       boxCollider = GetComponent<BoxCollider2D>();
       repeatWidth = boxCollider.size.x / 2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localPosition.x < startPosition.x - repeatWidth)
        {
            transform.localPosition = startPosition;
        }
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }


}
