using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class AiControlCar : MonoBehaviour
{
    [Header("Movement  (match ControlCar values)")]
    public float acceleration = 35f;
    public float maxSpeed = 25f;
    public float turnStrength = 120f;
    public float driftGrip = 0.95f;

    [Header("Downforce")]
    public float downForce = 50f;

    [Header("Wheels")]
    [SerializeField] GameObject[] Wheels = new GameObject[4];

    [Header("Waypoint Circuit")]
    public WaypointCircuit circuit;          // drag the circuit for this map here

    [Header("AI Tuning")]
    [Tooltip("How far ahead on the path the AI steers toward (higher = smoother)")]
    public float lookAheadDistance = 8f;

    [Tooltip("Speed at which the AI brakes before sharp corners (0 = no braking)")]
    public float cornerBrakeStrength = 0.6f;

    [Tooltip("Angle (deg) above which the AI starts braking")]
    public float cornerBrakeAngle = 25f;

    [Tooltip("Rubber-band: if this car is ahead of player by this many metres, reduce throttle")]
    public float rubberBandSlowDistance = 40f;
    [Tooltip("Rubber-band: if this car is behind player by this many metres, add throttle bonus")]
    public float rubberBandFastDistance = 40f;
    [Range(0f, 1f)]
    public float rubberBandStrength = 0.25f;

    [Tooltip("Small random variation added to look-ahead so AI lines aren't perfectly identical")]
    public float randomnessFactor = 1.2f;


    private Rigidbody rb;
    private float throttle;
    private float steer;
    private int targetWaypointIndex;
    private Transform playerTransform;       // found automatically
    private float randomOffset;          // per-instance lateral nudge

    // Wheel animation (same logic as ControlCar)
    private float _currentSteerAngle;
    private float MaxSteerAngle = 30f;
    private float SteerSmoothSpeed = 10f;

    public int RacingNumber { get; set; }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);

        // Give each AI a slight random line variation so they don't stack perfectly
        randomOffset = Random.Range(-randomnessFactor, randomnessFactor);

        // Try to find the player car (tagged "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) playerTransform = playerObj.transform;

        GameObject Waypoint = GameObject.FindGameObjectWithTag("Waypoint");
        if(!Waypoint.TryGetComponent<WaypointCircuit>(out circuit))
        {
            Debug.LogError("Couldnt Find the waypoints");
        }
    }

    private void Update()
    {
        if (circuit == null) return;

        ComputeSteeringInputs();
    }

    private void FixedUpdate()
    {
        ApplyAcceleration();
        ApplySteering();
        ApplyGrip();
        ApplyDownforce();
        ClampSpeed();
        AnimateWheels();
    }


    private void ComputeSteeringInputs()
    {
        // 1. Advance waypoint if close enough
        Vector3 targetPos = circuit.GetWaypointPosition(targetWaypointIndex);
        float distToWP = Vector3.Distance(transform.position, targetPos);

        if (distToWP < circuit.waypointReachRadius)
        {
            targetWaypointIndex = circuit.NextIndex(targetWaypointIndex);
        }

        // 2. Look-ahead: pick a point further along the path for smoother steering
        Vector3 lookTarget = circuit.GetLookAheadPoint(
            transform.position,
            targetWaypointIndex,
            lookAheadDistance + randomOffset);

        // 3. Steer toward look-ahead target
        Vector3 localTarget = transform.InverseTransformPoint(lookTarget);
        // Normalise by a fixed width rather than lookAheadDistance so steer values
        // are actually close to ±1 and the car commits to turns instead of under-steering.
        steer = Mathf.Clamp(localTarget.x / 5f, -1f, 1f);

        // 4. Throttle — base is full forward
        throttle = 1f;

        // 5. Corner braking: look at the angle to the NEXT waypoint after the target
        int nextIdx = circuit.NextIndex(targetWaypointIndex);
        Vector3 toTarget = (targetPos - transform.position).normalized;
        Vector3 toNext = (circuit.GetWaypointPosition(nextIdx) - targetPos).normalized;
        float cornerAngle = Vector3.Angle(toTarget, toNext);

        if (cornerAngle > cornerBrakeAngle)
        {
            float brakeAmount = Mathf.InverseLerp(cornerBrakeAngle, 90f, cornerAngle);
            throttle -= brakeAmount * cornerBrakeStrength;
        }

        // 6. Rubber-band against player (applied after braking, clamped separately)
        float rubberBonus = 0f;
        if (playerTransform != null)
        {
            float progressDiff = circuit.GetProgressDifference(transform.position, playerTransform.position);

            if (progressDiff > rubberBandSlowDistance)
                rubberBonus = -rubberBandStrength;
            else if (progressDiff < -rubberBandFastDistance)
                rubberBonus = rubberBandStrength;
        }

        // Clamp throttle and rubber separately so braking corners are never negated
        // and the rubber band can't push throttle below the braked value.
        throttle = Mathf.Clamp(throttle, 0f, 1f);
        throttle = Mathf.Clamp(throttle + rubberBonus, 0.1f, 1f); // never full-stop from rubber band
    }



    void ApplyAcceleration()
    {
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * throttle * acceleration, ForceMode.Acceleration);
        }
    }

    void ApplySteering()
    {
        // Clamp speed factor to a minimum of 0.3 so the AI can steer at low speeds
        // and actually build up velocity. Without this floor, early-corner corrections
        // are so weak the AI snakes and loses huge amounts of speed.
        float speedFactor = Mathf.Max(rb.linearVelocity.magnitude / maxSpeed, 0.3f);
        float turn = steer * turnStrength * speedFactor * Time.fixedDeltaTime;
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
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void AnimateWheels()
    {
        Vector3 localAngular = rb.transform.InverseTransformDirection(rb.angularVelocity);
        float spinSpeed = localAngular.x * Mathf.Rad2Deg;
        float delta = spinSpeed * Time.deltaTime;

        for (int i = 0; i < Wheels.Length; i++)
            Wheels[i].transform.Rotate(Vector3.right, delta, Space.Self);

        float targetAngle = steer * MaxSteerAngle;
        _currentSteerAngle = Mathf.Lerp(_currentSteerAngle, targetAngle, Time.deltaTime * SteerSmoothSpeed);

        Vector3 e0 = Wheels[0].transform.localEulerAngles;
        Vector3 e1 = Wheels[1].transform.localEulerAngles;
        e0.y = e1.y = _currentSteerAngle;
        Wheels[0].transform.localEulerAngles = e0;
        Wheels[1].transform.localEulerAngles = e1;
    }

    public void AssignCircuit(WaypointCircuit c)
    {
        circuit = c;
        targetWaypointIndex = circuit.GetNearestWaypointIndex(transform.position);
    }
}