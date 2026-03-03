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

    private Vector3 StartingPosition;
    private Quaternion StartingRotation;

    private float PreviousDistance;

    public override void Initialize()
    {
        rb= GetComponent<Rigidbody>();
        StartingPosition = transform.position;
        StartingRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        CurrentCheckpoint = 0;

        PreviousDistance = Vector3.Distance(transform.position, checkpoints[CurrentCheckpoint].position);

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rb.linearVelocity.magnitude / 20f);

        Vector3 dirToCheckpoint = (checkpoints[CurrentCheckpoint].position - transform.position).normalized;
        Vector3 localDir = transform.InverseTransformDirection(dirToCheckpoint);

        sensor.AddObservation(localDir);

        sensor.AddObservation(transform.forward);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        steer = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        throttle = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        //Reward system for moving
        float currentDistance = Vector3.Distance(transform.position, checkpoints[CurrentCheckpoint].position);
        float progress = PreviousDistance - currentDistance;
        AddReward(progress * 0.5f);
        PreviousDistance = currentDistance;

        //Penalty for Time
        AddReward(0.001f);
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

        ContenuousActions[0] = Input.GetAxis("Vertical");
        ContenuousActions[1] = Input.GetAxis("Horizontal");
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

    public void FixedUpdate()
    {
        //Turn and Steer
        ApplyAcceleration();
        ApplySteering();
        ApplyGrip();
        ApplyDownforce();
        ClampSpeed();

        Debug.Log("Throttle: " + throttle);
        Debug.Log("Steer: " + steer);
    }
}
