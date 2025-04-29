using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using Unity.VisualScripting;
using TMPro;

public class BirdAgent : Agent
{
    // Bird Variables
    [SerializeField] private float jumpStr = 1.0f;
    [SerializeField] private float jumpDelay = 0.25f;
    private Vector2 startPosition;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    private Rigidbody2D rb;
    private Animator animator;

    // State Variables
    private bool isJumping = false;
    private bool hasGun = false;
    private bool alreadyFailed = false;

    // References
    [SerializeField] private SpawnManager spawnManager;


    // ML-Agent Variables
    private int _currentEpisode = 0;
    private float _cummReward = 0f;
    float yValidSuccessRange = 4.0f;

    // Testing Labels
    [SerializeField] private TextMeshProUGUI currentScoreLabel;
    [SerializeField] private TextMeshProUGUI highscoreLabel;
    [SerializeField] private TextMeshProUGUI currentEpisodeLabel;
    [SerializeField] private TextMeshProUGUI closestPipeDistLabel;
    [SerializeField] private TextMeshProUGUI jumpsThisGameLabel;
    [SerializeField] private TextMeshProUGUI shotsThisGameLabel;
    [SerializeField] private TextMeshProUGUI cummAILabel;

    // Game vars
    private int _currentScore = 0;
    private int _highscore = 0;
    private int _jumps_this_game = 0;
    private int _totalGunShots = 0;

    Transform nextGap = null;

    public override void Initialize()
    {
        Debug.Log("Initialize");

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.localPosition;
    }


    public override void OnEpisodeBegin()
    {
        //Debug.Log("Episode Started");
        StopAllCoroutines();

        StartCoroutine(SamplePipeDist());
        _currentEpisode++;
        _cummReward = 0f;
        _currentScore = 0;
        _jumps_this_game = 0;
        TrainingManager.num_epsiodes++;

        // Reset Some UI
        currentScoreLabel.text = "Current Score: " + 0;
        currentEpisodeLabel.text = "Current Episode: " + _currentEpisode;
        jumpsThisGameLabel.text = "Jumps This Game: " + 0; 
        CheckHighScoreAndUpdateLabel();

        isJumping = false;
        hasGun = false;
        alreadyFailed = false;

        // Reset the Game
        transform.localPosition = startPosition;
        spawnManager.HandleStartEpisode();
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        if (nextGap != null)
        {
            float xDist = nextGap.position.x - transform.position.x;
            float yDiff = nextGap.position.y - transform.position.y;

            UpdateClosestGapLabel(yDiff);

            //Debug.Log("Next gap is at a distance of something... ");

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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[0] = 1;
        }
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
            AddReward(-0.01f); // Punish Jump Spam during jump delay
            _cummReward = GetCumulativeReward();
            UpdateCummRewardLabel();
        }
        else
        {
            StartCoroutine(JumpCoe());
        }
    }

    // Jump coroutine. This is how timing for jump works.


    private IEnumerator JumpCoe()
    {
        // Apply Jump
        isJumping = true;
        SoundManager.instance.PlaySound("Jump");
        animator.SetTrigger("jump_trig");
        rb.linearVelocityY = jumpStr;
        IncrementJumpsThisGame();

        // Wait
        yield return new WaitForSeconds(jumpDelay);

        // No lomger jumping
        isJumping = false;
    }

    private IEnumerator SamplePipeDist()
    {
        while (true)
        {
            // Get the next gap we can see
            nextGap = GetNextGap();
            
            if (nextGap != null)
            {
                CheckIfBadYDistance();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void CheckIfBadYDistance()
    {
        float yDiff = nextGap.position.y - transform.position.y;
        

        if (Mathf.Abs(yDiff) > yValidSuccessRange)
        {
            AddReward(yDiff * -0.005f);
        }
        else
        {
            AddReward(0.001f); // small positive reward for staying well aligned!
        }

        _cummReward = GetCumulativeReward();
        UpdateCummRewardLabel();
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
            CollectedGun();
            //Debug.Log("PowerUp Get!");
            SoundManager.instance.PlaySound("GetGun");
            Destroy(collision.gameObject);
        }
        // Player passed a pipe
        else if (collision.gameObject.CompareTag("ScoreTriggerZone"))
        {
            PassedPipe();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Lower Bound"))
        {
            FellOutOfBounds();
        }
        else if (collision.gameObject.CompareTag("Upper Bound"))
        {
            SmackedCeiling();
        }
    }

    private void CheckHighScoreAndUpdateLabel()
    {
        if (_currentScore > _highscore)
        {
            _highscore = _currentScore;
            highscoreLabel.text = "Highscore: " + _highscore;
        }
    }

    private void UpdateClosestGapLabel(float distX)
    {
        closestPipeDistLabel.text = "Closest Pipe Dist X: " + distX;
    }

    private void IncrementJumpsThisGame()
    {
        _jumps_this_game++;
        jumpsThisGameLabel.text = "Jumps This Game: " + _jumps_this_game;
    }

    Transform GetNextGap()
    {
        Transform closest = null;
        float closestXDist = float.MaxValue;

        foreach (Transform gap in spawnManager.pipeGaps)
        {
            float xDist = gap.position.x - transform.position.x;

            //Debug.Log("Found a pipe with xDist of " + xDist);

            if (xDist > 0 && xDist < closestXDist)
            {
                closestXDist = xDist;
                closest = gap;
            }
        }

        return closest;
    }

    private void CollectedGun()
    {
        hasGun = true;
        animator.SetBool("hasGun_bool", true);

        AddReward(0.5f);
        _cummReward = GetCumulativeReward();
        UpdateCummRewardLabel();

        ShootGun(); // Shoot right when grabbed
    }

    private void ShootGun()
    {
        Instantiate(bulletPrefab, transform.position, bulletPrefab.transform.rotation, transform.parent); // bullet spawn
        Instantiate(muzzleFlashPrefab, transform.position, muzzleFlashPrefab.transform.rotation, transform.parent); // muzzle flash
        hasGun = false;
        animator.SetBool("hasGun_bool", false);
        SoundManager.instance.PlaySound("GunShot");

        // Gunshots
        _totalGunShots++;
        shotsThisGameLabel.text = "Total Gun Shots: " + _totalGunShots;
    }

    // Reward progression
    private void PassedPipe()
    {
        //PlayerPassedPipe?.Invoke();
        _currentScore++;
        currentScoreLabel.text = "Current Score: " + _currentScore;
        AddReward(1.0f);
        _cummReward = GetCumulativeReward();
        UpdateCummRewardLabel();
    }

    // Punish Death
    private void HitPipe()
    {
        //Debug.Log("Hit Pipe");
        AddReward(-1.0f);
        _cummReward = GetCumulativeReward();
        alreadyFailed = true;
        CheckHighScoreAndUpdateLabel();
        UpdateCummRewardLabel();
        EndEpisode();
    }

    // Punish Death
    private void FellOutOfBounds()
    {
        //Debug.Log("Fell Out of Bounds");
        AddReward(-1.0f);
        _cummReward = GetCumulativeReward();
        alreadyFailed = true;
        CheckHighScoreAndUpdateLabel();
        UpdateCummRewardLabel();
        EndEpisode();
    }

    // Punish ceiling clinging
    private void SmackedCeiling()
    {
        //Debug.Log("Smacked Ceiling");
        AddReward(-0.05f);
        _cummReward = GetCumulativeReward();
        UpdateCummRewardLabel();
    }

    private void UpdateCummRewardLabel()
    {
        cummAILabel.text = "Cummulative AI Reward: " + _cummReward;
    }
}
