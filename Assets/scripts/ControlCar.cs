using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;

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

    [Header("Wheels")]
    [SerializeField] GameObject[] Wheels = new GameObject[4];

    private Rigidbody rb;
    private float throttle;
    private float steer;

    GamePlay Object;
    GameObject CheckPointAndStarts;

    private Rigidbody[] WheelRB = new Rigidbody[4];

    private float MaxSteerAngle = 30f;
    private float SteerSmoothSpeed = 10f;
    private float _currentSteerAngle = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        CheckPointAndStarts = GameObject.Find("CheckpointsandStarts");
        Object = CheckPointAndStarts.GetComponent<GamePlay>();

        for(int i = 0; i < WheelRB.Length; i++)
        {
            WheelRB[i] = Wheels[i].GetComponent<Rigidbody>();
        }
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
        AnimateWheels();
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

    void AnimateWheels()
    {
        Vector3 localAngularVelocity = rb.transform.InverseTransformDirection(rb.angularVelocity);
        float spinSpeed = localAngularVelocity.x * Mathf.Rad2Deg;
        float delta = spinSpeed * Time.deltaTime;

        Wheels[0].transform.Rotate(Vector3.right, delta, Space.Self);
        Wheels[1].transform.Rotate(Vector3.right, delta, Space.Self);
        Wheels[2].transform.Rotate(Vector3.right, delta, Space.Self);
        Wheels[3].transform.Rotate(Vector3.right, delta, Space.Self);


        float targetAngle = steer * MaxSteerAngle;
        _currentSteerAngle = Mathf.Lerp(_currentSteerAngle, targetAngle, Time.deltaTime);

        Vector3 euler1 = Wheels[0].transform.localEulerAngles;
        Vector3 euler2 = Wheels[1].transform.localEulerAngles;

        euler1.y = euler2.y = _currentSteerAngle;

        Wheels[0].transform.localEulerAngles = euler1;
        Wheels[1].transform.localEulerAngles = euler2;
    }
}


