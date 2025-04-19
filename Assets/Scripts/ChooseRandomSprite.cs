using UnityEngine;

public class ChooseRandomSprite : MonoBehaviour
{
    public Sprite[] sprites;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int spriteIndex = Random.Range(0, sprites.Length);
        GetComponent<SpriteRenderer>().sprite = sprites[spriteIndex];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
