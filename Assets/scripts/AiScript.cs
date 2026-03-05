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

    public override void Initialize()
    {
        rb= GetComponent<Rigidbody>();
        StartingPosition = transform.position;
        StartingRotation = transform.rotation;

        currentEpisode = 0;
        cumulativeReward = 0f;
    }

    public override void OnEpisodeBegin()
    {
        currentEpisode = 0;
        cumulativeReward = 0f;
        currentSteps = 0;

        //rb.linearVelocity = Vector3.zero;
        //rb.angularVelocity = Vector3.zero;

        CurrentCheckpoint = 0;

        PreviousDistance = Vector3.Distance(transform.position, checkpoints[CurrentCheckpoint].position);

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rb.linearVelocity.magnitude / 20f);

        Vector3 dirToCheckpoint = (checkpoints[CurrentCheckpoint].position - transform.position).normalized;
        Vector3 localDir = transform.InverseTransformDirection(dirToCheckpoint);

        //The Python backend running the ML agents has the sensors run on floats. So we are determing how the floats are formed.
        float[] localDirArray = { localDir.x, localDir.y, localDir.z };

        //sensor.AddObservation(localDir);

        //sensor.AddObservation(transform.forward);
        //sensor.AddObservation(checkpoints[CurrentCheckpoint].transform);
        sensor.AddObservation(localDirArray[0]);
        sensor.AddObservation(localDirArray[1]);
        sensor.AddObservation(localDirArray[2]);

        sensor.AddObservation(checkpoints[CurrentCheckpoint].transform.position.x/5f);
        sensor.AddObservation(checkpoints[CurrentCheckpoint].transform.position.y/5f);
        sensor.AddObservation(checkpoints[CurrentCheckpoint].transform.position.z/5f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Contenuous actions determined by Heuristic
        steer = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        throttle = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);

        //Reward system for moving
        float currentDistance = Vector3.Distance(transform.position, checkpoints[CurrentCheckpoint].position);
        float progress = PreviousDistance - currentDistance;
        AddReward(progress * 0.5f);
        PreviousDistance = currentDistance;

        //Penalty for Time
        AddReward(-0.001f);

        //The functions for moving the car. Same as player actions
        ApplyAcceleration();
        ApplySteering();
        ApplyGrip();
        ApplyDownforce();
        ClampSpeed();

        Debug.Log("Throttle: " + throttle);
        Debug.Log("Steer: " + steer);

        cumulativeReward = GetCumulativeReward();

        currentSteps++;
        if(currentSteps == MaxSteps)
        {
            AddReward(-2f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform == checkpoints[CurrentCheckpoint])
        {
            AddReward(1.0f);
            CurrentCheckpoint++;

            if(CurrentCheckpoint >= checkpoints.Count)
            {
                CurrentCheckpoint = 0;
                AddReward(5f);
                EndEpisode();
            }
            else
            {
                PreviousDistance = Vector3.Distance(transform.position, checkpoints[CurrentCheckpoint].position);
            }
        }

        /*
         if(other.CompareTag("Wall")
        {
            AddReward(-2f);
            EndEpisode();
        }
         */
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ContenuousActions = actionsOut.ContinuousActions;

        ContenuousActions[1] = Input.GetAxis("Vertical");
        ContenuousActions[2] = Input.GetAxis("Horizontal");
    }

    void ApplyAcceleration()
    {
        if (rb.linearVelocity.magnitude < maxspeed)
        {
            rb.AddForce(transform.forward * throttle * acceleration, ForceMode.Acceleration);
        }
    }

    void ApplySteering()
    {
        float speedfactor = rb.linearVelocity.magnitude / maxspeed;
        float turn = steer * turnstrength * speedfactor * Time.fixedDeltaTime;

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
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxspeed;
        }
    }
}
