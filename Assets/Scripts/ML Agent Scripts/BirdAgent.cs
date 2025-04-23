using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using Unity.VisualScripting;

public class BirdAgent : Agent
{
    // Bird Variables
    [SerializeField] private float jumpStr = 10.0f;
    [SerializeField] private float jumpDelay = 0.5f;
    private float upperBoundY = 4.5f;
    private float lowerBoundY = -6.0f;
    private Vector2 startPosition;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    private Rigidbody2D rb;
    private Animator animator;

    private bool isJumping = false;
    private bool hasGun = false;
    private bool alreadyFailed = false;

    // References
    [SerializeField] private SpawnManager spawnManager;


    // ML-Agent Variables
    private int _currentEpisode = 0;
    private float _cummReward = 0f;

    public override void Initialize()
    {
        Debug.Log("Initialize");

        startPosition = transform.localPosition;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.localPosition;
    }


    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Started");

        _currentEpisode++;
        _cummReward = 0f;

        isJumping = false;
        hasGun = false;
        alreadyFailed = false;

        // Reset the Game
        transform.localPosition = startPosition;
        spawnManager.HandleStartEpisode();
    }

    private void Update()
    {
        PlayerBoundCheckAndReact(); // Check to see if the player is ever out of bounds
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        // Get the next gap we can see
        Transform nextGap = GetNextGap();

        if (nextGap != null)
        {
            float xDist = nextGap.position.x - transform.position.x;
            float yDiff = nextGap.position.y - transform.position.y;

            sensor.AddObservation(xDist / 10f);   // normalize horizontally
            sensor.AddObservation(yDiff / 10f);    // normalize vertically
        }
        else
        {
            sensor.AddObservation(1f);  // no pipe? pretend one is far away
            sensor.AddObservation(0f);  // neutral vertical offset
        }

        // Get bird info
        float birdPosY_normalized = transform.localPosition.y / 6.5f;
        float birdVelocityY_normalized = rb.linearVelocityY / 6.5f;

        sensor.AddObservation(birdPosY_normalized);
        sensor.AddObservation(birdVelocityY_normalized);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        _cummReward = GetCumulativeReward();

        var action = actions.DiscreteActions[0];

        switch(action)
        {
            case 1:
                Jump();
                break;
        }
    }

    private void Jump() 
    { 
        if (isJumping)
        {
            AddReward(-0.05f); // Punish Jump Spam during jump delay
        }
        StartCoroutine(JumpCoe()); 
    }

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (alreadyFailed) return;  

        // Hit a Pipe
        if (collision.gameObject.CompareTag("Pipe"))
        {
            //Debug.Log("Pipe Detected");
            SoundManager.instance.PlaySound("Collision");
            HitPipe();
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
        else if (collision.gameObject.CompareTag("ScoreTriggerZone"))
        {
            PassedPipe();
        }
    }

    private void PlayerBoundCheckAndReact()
    {
        if (!alreadyFailed) return;

        // if we're above level upper bound
        if (transform.position.y > upperBoundY)
        {
            transform.position = new Vector2(transform.position.x, upperBoundY);
            SmackedCeiling();
        }
        // if we're below lower bound die
        else if (transform.position.y < lowerBoundY) FellOutOfBounds();
    }

    Transform GetNextGap()
    {
        Transform closest = null;
        float closestXDist = float.MaxValue;

        foreach (Transform gap in spawnManager.pipeGaps)
        {
            float xDist = gap.position.x - transform.position.x;

            if (xDist > 0 && xDist < closestXDist)
            {
                closestXDist = xDist;
                closest = gap;
            }
        }

        return closest;
    }

    // Reward progression
    private void PassedPipe()
    {
        //PlayerPassedPipe?.Invoke();

        AddReward(1.0f);
        _cummReward = GetCumulativeReward();
    }

    // Punish Death
    private void HitPipe()
    {
        AddReward(-1.0f);
        alreadyFailed = true;
        EndEpisode();
    }

    // Punish Death
    private void FellOutOfBounds()
    {
        AddReward(-1.0f);
        alreadyFailed = true;
        EndEpisode();
    }

    // Punish ceiling clinging
    private void SmackedCeiling()
    {
        AddReward(-0.05f);
    }
}
