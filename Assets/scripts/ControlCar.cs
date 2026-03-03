using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

/*public class ControlCar : MonoBehaviour
{
    [Header("Set in Inspector")]
    //350 Kilometers per hour
    public float carMaxSpeed = 350;
    //public float MaxReverseSpeed = 45f;
    public int AccelerationRate = 2; //2 meters per second squared (Maximum)
    public int DeccelerationRate = 2; //2 meters per second squared (Maximum)
    public int maxSteeringAngle = 28;
    //public float turningspeed = 2f;
    //public GameObject[] wheels;
    //public GameObject Player;
    public float smoothTime = 0.1f;
    public float turnSpeed = 100f;
    public GameObject RespawnPoint;
    Rigidbody carRigidbody;

    [Header("Set Dynamically")]
    float stearingAxis;
    float ThrottleAxis;

    Vector3 CurrentVelocity = Vector3.zero;
    Vector3 AccelerationVector;

    Quaternion currentRotationVector;

    private float targetSteeringAngle;
    private float currentSteeringAngle;
    private float smoothVelocity;
    private int stearingAngle;

    GamePlay Object;
    GameObject CheckPointAndStarts;


    private enum carStatus
    {
        Accelerating, Braking, Stopped, MovingNoAcceleration
    }

    carStatus currentCarStatus;

    public void Start()
    {
        currentRotationVector = gameObject.transform.rotation;
        RespawnPoint.transform.position = gameObject.transform.position;

        carRigidbody = gameObject.GetComponent<Rigidbody>();

        currentCarStatus = carStatus.Stopped;

        CheckPointAndStarts = GameObject.Find("CheckpointsandStarts");
        Object = CheckPointAndStarts.GetComponent<GamePlay>();
    }

    public void Update()
    {
        //Speed of car will be calculated by the magnitude of the velocity

        //Left and Right Steering
        stearingAxis = UnityEngine.Input.GetAxis("Horizontal");

        //Forward and backward movement
        ThrottleAxis = UnityEngine.Input.GetAxis("Vertical");


        if (ThrottleAxis != 0)
            CarMove();

        if(stearingAxis != 0)
            CarTurn();

        //Respawn point. Developer tool
        if(UnityEngine.Input.GetKeyDown("y"))
        {
            CarReset();
        }

        Object.CarSpeedChange(carRigidbody.linearVelocity);
    }

    private void CarMove()
    {
        if (ThrottleAxis != 0)
        {
            if (ThrottleAxis > 0)
            {
                //Creating the acceleration vector for the car
                //AccelerationVector = Vector3.forward * ThrottleAxis * AccelerationRate * Time.deltaTime * 100 + Vector3.forward;
                //Had a * Time.deltaTime * 100 + Vector3.forward
                AccelerationVector = Vector3.Lerp(AccelerationVector, Vector3.forward * AccelerationRate*200, Time.deltaTime * ThrottleAxis * AccelerationRate *0.01f);
                //AccelerationVector = Vector3.Lerp(AccelerationVector, Vector3.forward * carMaxSpeed * 100, Time.deltaTime * ThrottleAxis * AccelerationRate);

                currentCarStatus = carStatus.Accelerating;
            }
            //Debug.Log("Magnitude of current Velocity: " + CurrentVelocity.magnitude);
            if (carRigidbody.linearVelocity.magnitude > (carMaxSpeed))
            {
                //Ensures car doesnt go over max speed
                carRigidbody.linearVelocity = carRigidbody.linearVelocity.normalized * carMaxSpeed;
            }
            if (ThrottleAxis < 0)
            {
                //Car breaking (No forces applied just forcing the linear velocity to 0 quickly
                carRigidbody.linearVelocity = Vector3.Lerp(carRigidbody.linearVelocity, Vector3.zero, Time.deltaTime * DeccelerationRate / 2);
                currentCarStatus = carStatus.Braking;
                AccelerationVector = Vector3.zero;
            }
        }
        else if (ThrottleAxis == 0)
        {
            //The magnitude of the velocity vector of the car needs to approach and end at 0 
            //from wherever its at
            //Debug.Log("Current Velocity Magnitude: " + CurrentVelocity.magnitude);
            carRigidbody.linearVelocity = Vector3.Lerp(carRigidbody.linearVelocity, Vector3.zero, Time.deltaTime * 4);
            currentCarStatus = carStatus.MovingNoAcceleration;

            AccelerationVector = Vector3.zero;
        }

        if (carRigidbody.linearVelocity.magnitude < 0.0005)
        {
            carRigidbody.linearVelocity = Vector3.zero;
        }

        if (carRigidbody.linearVelocity == Vector3.zero)
        {
            currentCarStatus = carStatus.Stopped;
        }
        //gameObject.transform.Translate(CurrentVelocity);
        //Debug.Log(carRigidbody.linearVelocity.magnitude);
        carRigidbody.AddRelativeForce(AccelerationVector, ForceMode.Force);
    }

    private void CarTurn()
    {
        switch (currentCarStatus)
        {
            case carStatus.Accelerating:
                stearingAngle = maxSteeringAngle;
                break;
            case carStatus.Braking:
                stearingAngle = maxSteeringAngle / 2;
                break;
            case carStatus.MovingNoAcceleration:
                stearingAngle = maxSteeringAngle + 10;
                break;
            case carStatus.Stopped:
                stearingAngle = 0;
                break;
        }
        //Debug.Log(stearingAxis);
        targetSteeringAngle = stearingAxis * stearingAngle;


        currentSteeringAngle = Mathf.SmoothDamp(currentSteeringAngle, targetSteeringAngle, ref smoothVelocity, smoothTime);
        gameObject.transform.Rotate(0, currentSteeringAngle * Time.deltaTime, 0);
    }

    private void CarReset()
    {
        transform.position = RespawnPoint.transform.position;
        carRigidbody.linearVelocity = Vector3.zero;
        transform.rotation = currentRotationVector;
        Object.CarReset();
    }

}*/
[RequireComponent(typeof(Rigidbody))]
public class ControlCar : MonoBehaviour
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

    GamePlay Object;
    GameObject CheckPointAndStarts;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        CheckPointAndStarts = GameObject.Find("CheckpointsandStarts");
        Object = CheckPointAndStarts.GetComponent<GamePlay>();
    }

    private void Update()
    {
        throttle = UnityEngine.Input.GetAxis("Vertical");
        steer = UnityEngine.Input.GetAxis("Horizontal");

        Object.CarSpeedChange(rb.linearVelocity);
    }

    private void FixedUpdate()
    {
        ApplyAcceleration();
        ApplySteering();
        ApplyGrip();
        ApplyDownforce();
        ClampSpeed();
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
        if(rb.linearVelocity.magnitude > maxspeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxspeed;
        }
    }
}


