using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCar : MonoBehaviour
{
    //Inputs
    public InputActionAsset InputActions;
    InputAction accelerateInput;
    InputAction brakeInput;
    InputAction steeringInput;

    //Performance stats
    [Header("Performance Variables")]
    [SerializeField] private int horsePower = 500;

    [SerializeField] private int maxSteeringAngleDegrees = 23;

    //Wheel transforms & Colliders
    [SerializeField] private Transform FLWheelTransform;
    [SerializeField] private Transform FRWheelTransform;
    [SerializeField] private Transform RLWheelTransform;
    [SerializeField] private Transform RRWheelTransform;

    [SerializeField] private WheelCollider FLWheelCollider;
    [SerializeField] private WheelCollider FRWheelCollider;
    [SerializeField] private WheelCollider RLWheelCollider;
    [SerializeField] private WheelCollider RRWheelCollider;

    //Car running variables
    private float acceleratePedalPressure = 0;
    private float brakePedalPressure = 0;
    private float steeringValue = 0;

    private float carMotorForce = 0;
    private float carBrakeForce = 0;

    //Decorative
    [SerializeField] private Transform steeringWheelTransform;
    [SerializeField] private int maxDegreesRotationForSteeringWheel = 900;
    [SerializeField] private float rotateSpeed = 1.0f;
    private Vector3 defaultSteeringWheelRotation;

    private void Awake()
    {
        var vehicleMap = InputActions.FindActionMap("Vehicle");
        accelerateInput = vehicleMap.FindAction("Accelerate");
        brakeInput = vehicleMap.FindAction("Brake");
        steeringInput = vehicleMap.FindAction("Steering");

        defaultSteeringWheelRotation = steeringWheelTransform.localEulerAngles;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        RotateSteeringWheel();
    }

    private void OnEnable()
    {
        accelerateInput.Enable();
        brakeInput.Enable();
        steeringInput.Enable();
    }

    private void OnDisable()
    {
        accelerateInput.Disable();
        brakeInput.Disable();
        steeringInput.Disable();
    }

    private void FixedUpdate()
    {
        CalculateSteering();
        CalculateMotorForce();
        GiveWheelsPower();
        GiveWheelsBrakeForce();
    }
    private void GetInput()
    {
        acceleratePedalPressure = accelerateInput.ReadValue<float>();
        brakePedalPressure = brakeInput.ReadValue<float>();
        steeringValue = steeringInput.ReadValue<float>();
        Debug.Log($"Accelerate Pedal Pressure: {acceleratePedalPressure}");
        Debug.Log($"Brake Pedal Pressure: {brakePedalPressure}");
    }

    private void CalculateSteering()
    {
        var currentSteeringAngle = maxSteeringAngleDegrees * steeringValue;
        FLWheelCollider.steerAngle = currentSteeringAngle;
        FRWheelCollider.steerAngle = currentSteeringAngle;
    }

    private void RotateSteeringWheel()
    {
        /*Vector3 newRotation = defaultSteeringWheelRotation;
        newRotation.z += -steeringValue * maxDegreesRotationForSteeringWheel;

        steeringWheelTransform.localEulerAngles = newRotation;*/

        Vector3 targetEulerRotation = defaultSteeringWheelRotation;
        targetEulerRotation.z += -steeringValue * maxDegreesRotationForSteeringWheel;
        Quaternion targetRotation = Quaternion.Euler(targetEulerRotation);

        // Smoothly interpolate from current rotation to target rotation
        steeringWheelTransform.localRotation = Quaternion.Lerp(
            steeringWheelTransform.localRotation,
            targetRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    private void CalculateMotorForce()
    {
        carMotorForce = horsePower * acceleratePedalPressure;
        carBrakeForce = 3 * horsePower * brakePedalPressure;
    }

    private void GiveWheelsPower()
    {
        RLWheelCollider.motorTorque = carMotorForce;
        RRWheelCollider.motorTorque = carMotorForce;
    }

    private void GiveWheelsBrakeForce()
    {
        RLWheelCollider.brakeTorque = carBrakeForce;
        RRWheelCollider.brakeTorque = carBrakeForce;
    }

}
