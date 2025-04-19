using System;
using System.Collections;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private float jumpInput;
    [SerializeField] private float jumpStr = 10.0f;
    [SerializeField] private float jumpDelay = 0.5f;
    private float upperBoundY = 4.5f;
    private float lowerBoundY = -6.0f;
    private Vector2 startPosition;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    private Rigidbody2D rb;
    private Animator animator;

    // State variables
    private bool isGameActive = false;
    private bool isGameOver = false;
    private bool isJumping = false;
    private bool hasGun = false;

    public event Action PlayerPassedPipe;
    public event Action PlayerDied;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Events
        GameManager.Instance.OnGameStart += HandleGameStart;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameActive) // if the game is currently active, can control bird
        {
            jumpInput = Input.GetAxis("Jump");

            //Jump
            if (jumpInput >= .99f && !isJumping)
            {
                Jump();
            }
            // Shoot Gun
            else if (hasGun && (Input.GetKeyDown(KeyCode.Return) || (Input.GetKeyDown(KeyCode.Mouse0))))
            {
                ShootGun();
            }
            // Check if player is too high or too low and act accordingly
            PlayerBoundCheckAndReact();
        }
        else if (isGameOver) // If Game is Over
        {
            // Keep dead bird from falling forever
            if (transform.position.y < lowerBoundY) FreezePlayerPosition(new Vector2(transform.position.x, lowerBoundY));
        }
        else // If game hasn't started yet (We're on title screen)
        {
            // Freeze Player On Title Screen
            FreezePlayerPosition(startPosition);
            rb.linearVelocity = new Vector2(0, jumpStr); // Prevent velocity buildup and prepare jump strength (OOGA BOOGA)
        }

    }

    private void HandleGameStart()
    {
        isGameActive = true;

        // Handle Events
        GameManager.Instance.OnGameStart -= HandleGameStart;
        GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void HandleGameOver()
    {
        isGameOver = true;
        isGameActive = false;

        GameManager.Instance.OnGameOver -= HandleGameOver;
    }

    private void Jump(){StartCoroutine(JumpCoe());}

    // Jump coroutine. This is how timing for jump works.
    private IEnumerator JumpCoe()
    {
        // Apply Jump
        isJumping = true;
        SoundManager.instance.PlaySound("Jump");
        animator.SetTrigger("jump_trig");
        rb.linearVelocityY = jumpStr;

        // Wait
        yield return new WaitForSeconds(jumpDelay);

        // No lomger jumping
        isJumping = false;
    }

    private void ShootGun()
    {
        Instantiate(bulletPrefab, transform.position, bulletPrefab.transform.rotation); // bullet spawn
        Instantiate(muzzleFlashPrefab, transform.position, muzzleFlashPrefab.transform.rotation); // muzzle flash
        hasGun = false;
        animator.SetBool("hasGun_bool", false);
        SoundManager.instance.PlaySound("GunShot");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit a Pipe
        if (collision.gameObject.CompareTag("Pipe") && isGameActive)
        {
            //Debug.Log("Pipe Detected");
            SoundManager.instance.PlaySound("Collision");
            Die();
        }
        // Hit Gun Powerup
        else if (collision.gameObject.CompareTag("GunPU"))
        {
            //Debug.Log("PowerUp Get!");
            hasGun = true;
            animator.SetBool("hasGun_bool", true);
            SoundManager.instance.PlaySound("GetGun");
            Destroy(collision.gameObject);
        }
        // Player passed a pipe
        else if(collision.gameObject.CompareTag("ScoreTriggerZone"))
        {
            PlayerPassedPipe?.Invoke();
        }
    }

    private void PlayerBoundCheckAndReact()
    {
        // if we're above level upper bound
        if (transform.position.y > upperBoundY)
        {
            transform.position = new Vector2(transform.position.x, upperBoundY);
        }
        // if we're beow lower bound die
        else if (transform.position.y < lowerBoundY) Die();
    }

    private void Die()
    {
        PlayerDied?.Invoke();
        float deathBounceForce = 5f;
        rb.AddForce(Vector2.up * deathBounceForce, ForceMode2D.Impulse);
    }

    private void FreezePlayerPosition (Vector2 position)
    {
        transform.position = position;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStart -= HandleGameStart;
        GameManager.Instance.OnGameOver -= HandleGameOver;
    }
}
