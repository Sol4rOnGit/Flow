using System;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCar : MonoBehaviour
{
    //Inputs
    public InputActionAsset InputActions;
    InputAction accelerateInput;
    InputAction brakeInput;
    InputAction steeringInput;

    InputAction switchCameraInput;

    //Performance stats
    [Header("Performance Variables")]
    [SerializeField] private int engineForce = 500;
    [SerializeField] private int maxBrakeForce = 500;

    [SerializeField] private int maxSteeringAngleDegrees = 23;

    //Wheel transforms & Colliders
    [Header("Wheel transforms and Colliders")]
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

    //Cameras
    [Header("Cameras")]
    [SerializeField] private Camera[] Cameras;
    private int currentCameraIndex = 0;

    //Decorative
    [Header("Decorative")]
    [SerializeField] private Transform steeringWheelTransform;
    [SerializeField] private int maxDegreesRotationForSteeringWheel = 900;
    [SerializeField] private float rotateSpeed = 1.0f;
    private Vector3 defaultSteeringWheelRotation;

    private void Awake()
    {
        //Input mapping
        var vehicleMap = InputActions.FindActionMap("Vehicle");
        accelerateInput = vehicleMap.FindAction("Accelerate");
        brakeInput = vehicleMap.FindAction("Brake");
        steeringInput = vehicleMap.FindAction("Steering");

        switchCameraInput = vehicleMap.FindAction("SwitchCamera");

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
        CameraSwitch();
    }

    private void OnEnable()
    {
        accelerateInput.Enable();
        brakeInput.Enable();
        steeringInput.Enable();
        switchCameraInput.Enable();
    }

    private void OnDisable()
    {
        accelerateInput.Disable();
        brakeInput.Disable();
        steeringInput.Disable();
        switchCameraInput.Disable();
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
    }

    private void CalculateSteering()
    {
        var currentSteeringAngle = maxSteeringAngleDegrees * steeringValue;
        FLWheelCollider.steerAngle = currentSteeringAngle;
        FRWheelCollider.steerAngle = currentSteeringAngle;
    }

    private void CalculateMotorForce()
    {
        carMotorForce = engineForce * acceleratePedalPressure;
        carBrakeForce = maxBrakeForce * brakePedalPressure;
    }

    private void GiveWheelsPower()
    {
        RLWheelCollider.motorTorque = carMotorForce;
        RRWheelCollider.motorTorque = carMotorForce;
    }

    private void GiveWheelsBrakeForce()
    {
        FLWheelCollider.brakeTorque = carBrakeForce;
        FRWheelCollider.brakeTorque = carBrakeForce;
        RLWheelCollider.brakeTorque = carBrakeForce;
        RRWheelCollider.brakeTorque = carBrakeForce;
    }

    //Decorative
    private void RotateSteeringWheel()
    {
        //Get rotation
        Vector3 targetEulerRotation = defaultSteeringWheelRotation;
        targetEulerRotation.z += -steeringValue * maxDegreesRotationForSteeringWheel;
        Quaternion targetRotation = Quaternion.Euler(targetEulerRotation);

        //Interpolate
        steeringWheelTransform.localRotation = Quaternion.Lerp(
            steeringWheelTransform.localRotation,
            targetRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    //Camera switching
    private void CameraSwitch()
    {
        if (!switchCameraInput.WasPressedThisFrame())
        {
            return;
        }

        Debug.Log("switchCameraInput was pressed.");

        currentCameraIndex = (currentCameraIndex + 1) % Cameras.Length;

        EnableOnlyIndexedCamera();

    }

    private void EnableOnlyIndexedCamera()
    {
        for (int i = 0; i < Cameras.Length; i++){
            Cameras[i].gameObject.SetActive(false);
        }

        Cameras[currentCameraIndex].gameObject.SetActive(true);
    }
}