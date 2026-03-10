using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections.Generic;

public class AiScript : Agent
{
    [Header("Movement")]
    public float acceleration = 35f;
    public float maxspeed = 25f;
    public float turnstrength = 120f;
    public float driftGrip = 0.95f;

    [Header("Down Force")]
    public float downForce = 50f;

    private Rigidbody rb;
    private float throttle;
    private float steer;

    [Header("Track")]
    public List<Transform> checkpoints;
    private int CurrentCheckpoint;

    [Header("Training")]
    [SerializeField] public int MaxSteps = 0;

    private Vector3 StartingPosition;
    private Quaternion StartingRotation;

    private float PreviousDistance;

    private int currentEpisode = 0;
    private float cumulativeReward = 0f;
    private int currentSteps = 0;


    [Header("Reward Weights")]
    [Tooltip("Multiplier for distance-progress reward each step")]
    public float progressRewardScale = 1.0f;

    [Tooltip("Reward for passing a checkpoint")]
    public float checkpointReward = 10.0f;

    [Tooltip("Bonus reward for completing a full lap")]
    public float lapCompletionBonus = 20.0f;

    [Tooltip("Penalty per step for sitting still (speed below idle threshold)")]
    public float idlePenaltyPerStep = -0.005f;

    [Tooltip("Speed below this (m/s) is considered idle")]
    public float idleSpeedThreshold = 1.5f;

    [Tooltip("Penalty on first wall contact")]
    public float wallHitPenalty = -2.0f;

    [Tooltip("Penalty per step while grinding a wall")]
    public float wallGrindPenalty = -0.5f;

    [Tooltip("How strongly heading alignment is rewarded (0 = off)")]
    public float headingAlignScale = 0.3f;

    [Tooltip("Per-step time penalty to encourage speed (set 0 to disable)")]
    public float timePenaltyPerStep = -0.001f;


    private bool touchingWall = false;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        StartingPosition = transform.position;
        StartingRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        currentEpisode++;
        cumulativeReward = 0f;
        currentSteps = 0;
        touchingWall = false;


        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = StartingPosition;
        transform.rotation = StartingRotation;

        CurrentCheckpoint = 0;
        PreviousDistance = Vector3.Distance(transform.position, checkpoints[CurrentCheckpoint].position);

        Debug.Log($"=== Episode {currentEpisode} Begin ===");
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        sensor.AddObservation(rb.linearVelocity.magnitude / maxspeed);


        Vector3 dirToCheckpoint = (checkpoints[CurrentCheckpoint].position - transform.position).normalized;
        sensor.AddObservation(transform.InverseTransformDirection(dirToCheckpoint));


        sensor.AddObservation(transform.forward);


        float dist = Vector3.Distance(transform.position, checkpoints[CurrentCheckpoint].position);
        sensor.AddObservation(dist / 100f);   


    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        throttle = Mathf.Clamp(actions.ContinuousActions[0], 0f, 1f);  
        steer = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);


        ApplyAcceleration();
        ApplySteering();
        ApplyGrip();
        ApplyDownforce();
        ClampSpeed();



 
        float currentDistance = Vector3.Distance(transform.position, checkpoints[CurrentCheckpoint].position);
        float progress = PreviousDistance - currentDistance;
        AddReward(progress * progressRewardScale);
        PreviousDistance = currentDistance;


        Vector3 toCheckpoint = (checkpoints[CurrentCheckpoint].position - transform.position).normalized;
        float alignment = Vector3.Dot(transform.forward, toCheckpoint); 
        AddReward(alignment * headingAlignScale * Time.fixedDeltaTime);


        if (rb.linearVelocity.magnitude < idleSpeedThreshold)
            AddReward(idlePenaltyPerStep);


        AddReward(timePenaltyPerStep);


        currentSteps++;
        cumulativeReward = GetCumulativeReward();

        if (currentSteps % 200 == 0)
            Debug.Log($"[Ep {currentEpisode}] Step {currentSteps} | Reward {cumulativeReward:F3} | Speed {rb.linearVelocity.magnitude:F1} m/s | CP {CurrentCheckpoint}/{checkpoints.Count}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != checkpoints[CurrentCheckpoint]) return;

        AddReward(checkpointReward);
        CurrentCheckpoint++;

        if (CurrentCheckpoint >= checkpoints.Count)
        {

            AddReward(lapCompletionBonus);
            Debug.Log($"[Ep {currentEpisode}] LAP COMPLETE — total reward: {GetCumulativeReward():F2}");
            EndEpisode();
        }
        else
        {
            PreviousDistance = Vector3.Distance(transform.position, checkpoints[CurrentCheckpoint].position);
            Debug.Log($"[Ep {currentEpisode}] Checkpoint {CurrentCheckpoint}/{checkpoints.Count} reached");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Wall":
                AddReward(wallHitPenalty);
                touchingWall = true;
                break;

            case "Track":

                AddReward(0.00001f);
                break;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
            AddReward(wallGrindPenalty * Time.fixedDeltaTime); 
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
            touchingWall = false;
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Vertical");    
        ca[1] = Input.GetAxis("Horizontal");  
    }


    void ApplyAcceleration()
    {
        if (rb.linearVelocity.magnitude < maxspeed)
            rb.AddForce(transform.forward * throttle * acceleration, ForceMode.Acceleration);
    }

    void ApplySteering()
    {
        float speedFactor = rb.linearVelocity.magnitude / maxspeed;
        float turn = steer * turnstrength * speedFactor * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }

    void ApplyGrip()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        localVelocity.x *= driftGrip;
        rb.linearVelocity = transform.TransformDirection(localVelocity);
    }

    void ApplyDownforce()
    {
        rb.AddForce(-transform.up * downForce * rb.linearVelocity.magnitude);
    }

    void ClampSpeed()
    {
        if (rb.linearVelocity.magnitude > maxspeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxspeed;
    }
}
