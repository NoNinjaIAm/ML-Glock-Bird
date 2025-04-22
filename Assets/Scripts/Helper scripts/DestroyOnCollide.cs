using UnityEngine;

public class DestroyOnCollide : MonoBehaviour
{
    public ParticleSystem destroyEffect;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pipe"))
        {
            // Sound
            SoundManager.instance.PlaySound("PipeDestroyed");

            // Spawn Particles
            Instantiate(destroyEffect.gameObject, transform.position, destroyEffect.transform.rotation);

            //Debug.Log("Pipe Detected");
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
