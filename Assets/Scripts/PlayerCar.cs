using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerCar : MonoBehaviour
{
    //Inputs
    public InputActionAsset InputActions;
    InputAction accelerateInput;
    InputAction brakeInput;
    InputAction steeringInput;

    InputAction gearUpInput;
    InputAction gearDownInput;

    InputAction switchCameraInput;
    InputAction resetCarInput;
    private bool wasResetCarInputPressed;

    //Performance stats
    [Header("Performance Variables")]
    [SerializeField] private int engineForce = 500;
    [SerializeField] private int maxBrakeForce = 500;

    [SerializeField] private int maxSteeringAngleDegrees = 23;
    private float minSteeringAngle = 9f;
    [SerializeField] private bool arcadeHandling = true;

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

    //Other Car Variables
    [Header("Other Car Variables")]
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Quaternion originalRotation;
    private Rigidbody rb;

    //Car running variables
    private float acceleratePedalPressure = 0;
    private float brakePedalPressure = 0;
    private float steeringValue = 0;

    private float velocityms;
    private float velocitymph;

    private enum AutoGear
    {
        Reverse = -1,
        Neutral = 0,
        Forward = 1
    }
    private AutoGear currentGear = AutoGear.Forward;

    private float carMotorForce = 0;
    private float carBrakeForce = 0;

    //Cameras
    [Header("Cameras")]
    [SerializeField] private Camera[] Cameras;
    private int currentCameraIndex = 0;

    //UI
    [Header("UI")]
    [SerializeField] private TMP_Text speedTextDisplay;
    [SerializeField] private TMP_Text gearTextDisplay;
    [SerializeField] private Slider accelerationDisplay;
    [SerializeField] private Slider brakeDisplay;
    [SerializeField] private Slider clutchDisplay;
    [SerializeField] private Slider steeringDisplay;

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

        gearUpInput = vehicleMap.FindAction("GearUp");
        gearDownInput = vehicleMap.FindAction("GearDown");

        switchCameraInput = vehicleMap.FindAction("SwitchCamera");
        resetCarInput = vehicleMap.FindAction("ResetVehicle");

        //Car Parts
        rb = GetComponent<Rigidbody>();

        //Decor
        defaultSteeringWheelRotation = steeringWheelTransform.localEulerAngles;
    }

    void Start()
    {
        originalPosition = this.gameObject.transform.position;
        originalRotation = this.gameObject.transform.rotation;
    }

    void Update()
    {
        velocityms = rb.linearVelocity.magnitude;
        velocitymph = 2.23693629f * velocityms;

        GetInput();
        ChangeGearAutomatic();
        RotateSteeringWheel();

        //ProduceDownforce();

        //UI
        UpdateUI();

        //Decor
        CameraSwitch();
        UpdateAudio();
    }

    private void FixedUpdate()
    {
        CalculateSteering();
        CalculateMotorForce();
        GiveWheelsPower();
        GiveWheelsBrakeForce();
        UpdateAllWheels();
        ResetCar();
    }
    private void OnEnable()
    {
        accelerateInput.Enable();
        brakeInput.Enable();
        steeringInput.Enable();

        gearUpInput.Enable();
        gearDownInput.Enable();

        switchCameraInput.Enable();
        resetCarInput.Enable();
    }

    private void OnDisable()
    {
        accelerateInput.Disable();
        brakeInput.Disable();
        steeringInput.Disable();

        gearUpInput.Disable();
        gearDownInput.Disable();

        switchCameraInput.Disable();
        resetCarInput.Disable();
    }

    private void GetInput()
    {
        acceleratePedalPressure = accelerateInput.ReadValue<float>();
        brakePedalPressure = brakeInput.ReadValue<float>();
        steeringValue = steeringInput.ReadValue<float>();

        if (resetCarInput.WasPressedThisFrame())
        {
            wasResetCarInputPressed = true;
        }
    }
    
    private void CalculateSteering()
    {
        float currentSteeringAngle = maxSteeringAngleDegrees;

        //If arcade handling - reduce the steering angle with speed factor
        if (arcadeHandling)
        {
            float speedFactor = Mathf.Clamp01(velocitymph/70);

            float curve = Mathf.Pow(speedFactor, 2);

            float currentMaxAngle = Mathf.Lerp(maxSteeringAngleDegrees, minSteeringAngle, curve);

            currentSteeringAngle = currentMaxAngle * steeringValue;
        }

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
        RLWheelCollider.motorTorque = 0;
        RRWheelCollider.motorTorque = 0;

        if(currentGear == AutoGear.Neutral) { return; }

        if (currentGear == AutoGear.Forward)
        {
            RLWheelCollider.motorTorque = carMotorForce;
            RRWheelCollider.motorTorque = carMotorForce;
        }

        if (currentGear == AutoGear.Reverse) {
            if (rb.linearVelocity.magnitude < 9) {
                RLWheelCollider.motorTorque = -carMotorForce;
                RRWheelCollider.motorTorque = -carMotorForce;
            }
        }
    }

    private void GiveWheelsBrakeForce()
    {
        FLWheelCollider.brakeTorque = carBrakeForce;
        FRWheelCollider.brakeTorque = carBrakeForce;
        RLWheelCollider.brakeTorque = carBrakeForce;
        RRWheelCollider.brakeTorque = carBrakeForce;
    }

    private void ProduceDownforce()
    {
        Vector3 downforce = -transform.up * velocityms * velocityms * 0.9f;
        rb.AddForce(downforce);
    }

    private void UpdateAllWheels()
    {
        UpdateSingleWheel(FLWheelCollider, FLWheelTransform);
        UpdateSingleWheel(FRWheelCollider, FRWheelTransform);
        UpdateSingleWheel(RLWheelCollider, RLWheelTransform);
        UpdateSingleWheel(RRWheelCollider, RRWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position;
        Quaternion rotation;

        wheelCollider.GetWorldPose(out position, out rotation);
        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }

    private void ChangeGearAutomatic()
    {
        //Debug.Log(currentGear);

        if (gearUpInput.WasPressedThisFrame())
        {
            if (currentGear == AutoGear.Reverse)
            {
                currentGear = AutoGear.Neutral; return;
            }

            if (currentGear == AutoGear.Neutral)
            {
                currentGear = AutoGear.Forward;
            }
        }


        if (gearDownInput.WasPressedThisFrame())
        {
            if (currentGear == AutoGear.Neutral) { 
                currentGear = AutoGear.Reverse;
            }

            if (currentGear == AutoGear.Forward)
            {
                currentGear = AutoGear.Neutral;
            }
        }
    }

    //Reset Car
    private void ResetCar()
    {
        if (wasResetCarInputPressed)
        {
            ResetWheelColliders();

            //Teleport to spawn location
            rb.position = originalPosition;
            rb.rotation = originalRotation;

            //Set gear to 0
            currentGear = AutoGear.Neutral;

            //Reset inertia
            rb.linearVelocity = new Vector3(0f, 0f, 0f);
            rb.angularVelocity = new Vector3(0f, 0f, 0f);
            rb.Sleep();

            wasResetCarInputPressed = false;

            //this.gameObject.transform.position = originalPosition;
            //this.gameObject.transform.rotation = originalRotation;
        }
    }

    private void ResetWheelColliders()
    {
        WheelCollider[] wheels =
        {
            FLWheelCollider,
            FRWheelCollider,
            RLWheelCollider,
            RRWheelCollider
        };

        foreach (var wheel in wheels)
        {
            wheel.motorTorque = 0f;
            wheel.brakeTorque = 10000f; // hard lock
            wheel.steerAngle = 0f;
        }
    }


    //UI
    private void UpdateUI()
    {
        float roundedvelocitymph = (float)Math.Round(velocitymph * 10f) / 10f;

        speedTextDisplay.text = $"{roundedvelocitymph} mph";

        if (currentGear == AutoGear.Reverse) { gearTextDisplay.text = "R"; };
        if (currentGear == AutoGear.Neutral) { gearTextDisplay.text = "N"; };
        if (currentGear == AutoGear.Forward) { gearTextDisplay.text = "D"; };

        UpdateInputUI();
    }

    private void UpdateInputUI()
    {
        accelerationDisplay.value = acceleratePedalPressure;
        brakeDisplay.value = brakePedalPressure;
        steeringDisplay.value = steeringValue;
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

    private void UpdateAudio()
    {
        engineSound.pitch = Math.Min(1, rb.linearVelocity.magnitude / 80);

        //Fake gears for the time being

        if(currentGear == AutoGear.Reverse)
        {
            engineSound.pitch = Math.Min(1, rb.linearVelocity.magnitude / 10); 
            return;
        }

        if(velocityms < 1) { engineSound.pitch = 0.2f; return; }
        if(velocityms < 9) { engineSound.pitch = Math.Min(1, rb.linearVelocity.magnitude / 9); return; }
        if(velocityms < 16) { engineSound.pitch = Math.Min(1, rb.linearVelocity.magnitude / 16); return; }
        if(velocityms < 25) { engineSound.pitch = Math.Min(1, rb.linearVelocity.magnitude / 25); return; }
        if(velocityms < 35) { engineSound.pitch = Math.Min(1, rb.linearVelocity.magnitude / 35); return; }
        if(velocityms < 50) { engineSound.pitch = Math.Min(1, rb.linearVelocity.magnitude / 50); return; }
        if(velocityms < 80) { engineSound.pitch = Math.Min(1, rb.linearVelocity.magnitude / 80); return; }
    }
}