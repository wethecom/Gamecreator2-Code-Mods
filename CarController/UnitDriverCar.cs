using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;

[Title("Car Driver")]
[Image(typeof(IconCapsuleSolid), ColorTheme.Type.Green)]
[Category("Car Driver")]
[Description("Driver for Car-like movement")]

[Serializable]
public class UnitDriverCar : TUnitDriver
{
    [Header("Wheels")]
    [SerializeField] public List<WheelCollider> driveWheels = new List<WheelCollider>();
    [SerializeField] public List<WheelCollider> steeringWheels = new List<WheelCollider>();
    [SerializeField] public List<WheelCollider> allWheels = new List<WheelCollider>();
    [SerializeField] public List<Transform> wheelMeshes = new List<Transform>();

    [Header("Engine")]
    [Range(500, 3000)]
    [SerializeField] public float maxMotorTorque = 1500f;
    [Range(10, 100)]
    [SerializeField] public float maxBrakeTorque = 50f;
    [Range(10, 45)]
    [SerializeField] public float maxSteeringAngle = 30f;
    [Range(1, 10)]
    [SerializeField] public float differentialRatio = 3.5f;
    [Range(1, 10)]
    [SerializeField] public float gearRatio = 2.5f;
    [Range(0.1f, 1f)]
    [SerializeField] public float downforceCoefficient = 0.5f;

    [Header("Transmission")]
    [SerializeField]
    public AnimationCurve torqueCurve = new AnimationCurve(
        new Keyframe(0f, 0.5f),
        new Keyframe(0.2f, 0.8f),
        new Keyframe(0.5f, 1f),
        new Keyframe(0.8f, 0.8f),
        new Keyframe(1f, 0.5f)
    );
    [Range(1000, 8000)]
    [SerializeField] public float maxRPM = 5000f;
    [Range(800, 2000)]
    [SerializeField] public float idleRPM = 1000f;
    [Range(0.1f, 1f)]
    [SerializeField] public float clutchSlipThreshold = 0.3f;
    [SerializeField] public int numberOfGears = 5;
    [SerializeField] public float[] gearRatios;
    [SerializeField] public float reverseGearRatio = 3.5f;

    [Header("Handling")]
    [Range(0.1f, 1f)]
    [SerializeField] public float steeringSpeed = 0.5f;
    [Range(0.1f, 1f)]
    [SerializeField] public float steeringResetSpeed = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] public float tractionControl = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] public float stabilityControl = 0.5f;
    [Range(0.1f, 1f)]
    [SerializeField] public float antiRollAmount = 0.5f;

    [Header("Sound Effects")]
    [SerializeField] public AudioSource engineAudioSource;
    [SerializeField] public AudioSource skidAudioSource;
    [SerializeField] public AudioSource crashAudioSource;
    [SerializeField] public AudioSource gearShiftAudioSource;
    [SerializeField] public AudioClip engineStartClip;
    [SerializeField] public AudioClip engineRunningClip;
    [SerializeField] public AudioClip skidClip;
    [SerializeField] public AudioClip crashClip;
    [SerializeField] public AudioClip gearShiftClip;
    [Range(0.5f, 2f)]
    [SerializeField] public float minEnginePitch = 0.8f;
    [Range(1f, 3f)]
    [SerializeField] public float maxEnginePitch = 2f;
    [Range(0f, 1f)]
    [SerializeField] public float skidThreshold = 0.4f;
    [Range(0f, 5f)]
    [SerializeField] public float burnoutFactor = 2f;

    [Header("Particle Effects")]
    [SerializeField] public ParticleSystem[] wheelSmoke;
    [SerializeField] public TrailRenderer[] skidTrails;

    [Header("Debug")]
    [SerializeField] public bool showDebugInfo = false;

    // Private variables
    [SerializeField] public Rigidbody rb;
    private float currentSteeringAngle = 0f;
    private float currentBrakeForce = 0f;
    private float currentMotorTorque = 0f;
    private float currentRPM = 0f;
    private int currentGear = 1;
    private float currentSpeed = 0f;
    private bool isEngineRunning = false;
    private float slipRatio = 0f;
    private float burnoutIntensity = 0f;
    private Dictionary<WheelCollider, WheelData> wheelDataMap = new Dictionary<WheelCollider, WheelData>();
    private Vector3 lastPosition;
    private float lastUpdateTime;
    private bool isInitialized = true;

    // GameCreator required properties
    public override Vector3 WorldMoveDirection => rb != null ? rb.linearVelocity : Vector3.zero;
    public override Vector3 LocalMoveDirection => this.Transform.InverseTransformDirection(this.WorldMoveDirection);
    public override float SkinWidth => 0.1f;
    public override bool IsGrounded => CheckGrounded();

    private bool CheckGrounded()
    {
        foreach (var wheel in allWheels)
        {
            if (wheel.GetGroundHit(out _))
                return true; // At least one wheel is touching the ground
        }
        return false; // No wheels are in contact with the ground
    }

    public override Vector3 FloorNormal => Vector3.up;
    public override bool Collision { get; set; } = true;
    public override Axonometry Axonometry { get; set; }

    private class WheelData
    {
        public Transform wheelMesh;
        public ParticleSystem smoke;
        public TrailRenderer skidTrail;
        public bool isSkidding = false;
        public float slipRatio = 0f;
        public float lastRotation = 0f;
        public float angularVelocity = 0f;
    }

    // GameCreator lifecycle methods
    public override void OnStartup(Character character)
    {
        base.OnStartup(character);
        // InitializeCarComponents();

        rb = ShortcutPlayer.Transform.gameObject.GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // Lower center of mass for better stability
                                                    // Initialize gear ratios if not set
        if (gearRatios == null || gearRatios.Length != numberOfGears)
        {
            gearRatios = new float[numberOfGears];
            float ratio = 3.5f;
            for (int i = 0; i < numberOfGears; i++)
            {
                gearRatios[i] = ratio;
                ratio -= 0.5f;
                if (ratio < 0.8f) ratio = 0.8f;
            }
        }
        // Initialize wheel data
        for (int i = 0; i < allWheels.Count; i++)
        {
            WheelData data = new WheelData();
            if (i < wheelMeshes.Count) data.wheelMesh = wheelMeshes[i];
            if (i < wheelSmoke.Length) data.smoke = wheelSmoke[i];
            if (i < skidTrails.Length) data.skidTrail = skidTrails[i];
            wheelDataMap.Add(allWheels[i], data);
        }
        // Start engine
        StartEngine();
    }
    public override void OnDispose(Character character)
    {
        base.OnDispose(character);
        // Clean up any resources
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!isInitialized) return;

        UpdateWheelMeshes();
        HandleSounds();
        HandleParticleEffects();

        // Debug info
        if (showDebugInfo)
        {
            Debug.Log($"Speed: {currentSpeed:F1} km/h | RPM: {currentRPM:F0} | Gear: {(currentGear == 0 ? "R" : currentGear.ToString())}");
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (!isInitialized || rb == null) return;

        


        // Get input
        float throttleInput = Input.GetAxis("Vertical");
        float steeringInput = Input.GetAxis("Horizontal");
        bool brakeInput = Input.GetKey(KeyCode.Space);
        bool handbrakeInput = Input.GetKey(KeyCode.LeftShift);
        // Apply physics
        HandleSteering(steeringInput);
        HandleMotor(throttleInput);
        HandleBraking(brakeInput, handbrakeInput);
        CalculateEngineRPM();
        ApplyDownforce();
        ApplyAntiRoll();
        UpdateWheelData();
        // Calculate current speed
        currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert to km/h


    }

   
    public void Jump(float force,Character character)
    {
        
        // Cars don't jump, but we could implement a hydraulic system
        if (rb != null)
        {
            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        }
    }

    public override void SetPosition(Vector3 position)
    {
        if (rb != null)
        {
            rb.position = position;
        }
        else
        {
            this.Transform.position = position;
        }
    }

    public override void SetRotation(Quaternion rotation)
    {
        if (rb != null)
        {
            rb.rotation = rotation;
        }
        else
        {
            this.Transform.rotation = rotation;
        }
    }

    public override void AddPosition(Vector3 amount)
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + amount);
        }
        else
        {
            this.Transform.position += amount;
        }
    }

    public override void AddRotation(Quaternion amount)
    {
        if (rb != null)
        {
            rb.MoveRotation(rb.rotation * amount);
        }
        else
        {
            this.Transform.rotation *= amount;
        }
    }

    public override void SetScale(Vector3 scale)
    {
        this.Transform.localScale = scale;
    }

    public override void AddScale(Vector3 scale)
    {
        this.Transform.localScale += scale;
    }

    public override void ResetVerticalVelocity()
    {
        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;
        }
    }

    private void HandleSteering(float steeringInput)
    {
        // Gradually adjust steering angle for more realistic feel
        float targetSteeringAngle = steeringInput * maxSteeringAngle;
        // Reduce steering angle at higher speeds
        float speedFactor = Mathf.Clamp01(currentSpeed / 100f);
        targetSteeringAngle *= (1f - (speedFactor * 0.5f));
        if (steeringInput != 0)
        {
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetSteeringAngle, steeringSpeed);
        }
        else
        {
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, 0, steeringResetSpeed);
        }
        // Apply steering to wheels
        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = currentSteeringAngle;
        }
    }
    private void HandleMotor(float throttleInput)
    {
        // Calculate effective torque based on RPM curve
        float normalizedRPM = Mathf.Clamp01(currentRPM / maxRPM);
        float torqueMultiplier = torqueCurve.Evaluate(normalizedRPM);
        // Calculate gear ratio effect
        float effectiveGearRatio = (currentGear == 0) ? reverseGearRatio : gearRatios[Mathf.Abs(currentGear) - 1];
        float finalRatio = effectiveGearRatio * differentialRatio;
        // Calculate motor torque with input
        currentMotorTorque = throttleInput * maxMotorTorque * torqueMultiplier;
        // Apply torque to drive wheels
        foreach (WheelCollider wheel in driveWheels)
        {
            // Apply traction control
            WheelHit hit;
            float adjustedTorque = currentMotorTorque;
            if (wheel.GetGroundHit(out hit))
            {
                if (wheelDataMap[wheel].slipRatio > tractionControl)
                {
                    adjustedTorque *= (1f - (wheelDataMap[wheel].slipRatio - tractionControl));
                }
            }
            // Apply torque with gear ratio
            if (currentGear == 0) // Reverse
            {
                wheel.motorTorque = adjustedTorque * finalRatio * -1;
            }
            else
            {
                wheel.motorTorque = adjustedTorque * finalRatio;
            }
        }
        // Auto-shift gears
        AutoShiftGears();
    }
    private void HandleBraking(bool brakeInput, bool handbrakeInput)
    {
        // Regular braking
        if (brakeInput)
        {
            currentBrakeForce = maxBrakeTorque;
            foreach (WheelCollider wheel in allWheels)
            {
                wheel.brakeTorque = currentBrakeForce;
            }
        }
        // Handbrake (rear wheels only)
        else if (handbrakeInput)
        {
            currentBrakeForce = maxBrakeTorque * 1.5f;
            foreach (WheelCollider wheel in driveWheels)
            {
                wheel.brakeTorque = currentBrakeForce;
            }
            foreach (WheelCollider wheel in steeringWheels)
            {
                wheel.brakeTorque = 0;
            }
        }
        // No braking
        else
        {
            currentBrakeForce = 0f;
            foreach (WheelCollider wheel in allWheels)
            {
                wheel.brakeTorque = 0f;
            }
        }
    }
    private void CalculateEngineRPM()
    {
        float avgRPM = 0f;
        int wheelCount = 0;
        foreach (WheelCollider wheel in driveWheels)
        {
            avgRPM += wheel.rpm;
            wheelCount++;
        }
        if (wheelCount > 0)
        {
            avgRPM /= wheelCount;
            // Calculate effective gear ratio
            float effectiveGearRatio = (currentGear == 0) ? reverseGearRatio : gearRatios[Mathf.Abs(currentGear) - 1];
            float finalRatio = effectiveGearRatio * differentialRatio;
            // Calculate engine RPM from wheel RPM
            float targetRPM = Mathf.Abs(avgRPM * finalRatio * 60f / (2f * Mathf.PI));
            // Add clutch slip effect
            if (Mathf.Abs(Input.GetAxis("Vertical")) < clutchSlipThreshold)
            {
                targetRPM = Mathf.Lerp(targetRPM, idleRPM, 1f - Mathf.Abs(Input.GetAxis("Vertical")) / clutchSlipThreshold);
            }
            // Idle RPM when stationary
            if (currentSpeed < 1f && Mathf.Abs(Input.GetAxis("Vertical")) < 0.1f)
            {
                targetRPM = idleRPM;
            }
            // Smoothly adjust RPM
            currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.fixedDeltaTime * 5f);
            // Clamp RPM
            currentRPM = Mathf.Clamp(currentRPM, idleRPM, maxRPM * 1.1f);
        }
    }
    private void AutoShiftGears()
    {
        // Only shift if we're not in reverse
        if (currentGear >= 1)
        {
            // Shift up
            if (currentRPM >= maxRPM * 0.9f && currentGear < numberOfGears)
            {
                ShiftGear(currentGear + 1);
            }
            // Shift down
            else if (currentRPM <= maxRPM * 0.3f && currentGear > 1)
            {
                ShiftGear(currentGear - 1);
            }
        }
        // Handle reverse
        if (currentSpeed < 5f && Input.GetAxis("Vertical") < -0.5f && currentGear != 0)
        {
            ShiftGear(0); // Shift to reverse
        }
        else if (currentSpeed < 5f && Input.GetAxis("Vertical") > 0.1f && currentGear == 0)
        {
            ShiftGear(1); // Shift to first from reverse
        }
    }
    private void ShiftGear(int newGear)
    {
        if (newGear != currentGear)
        {
            currentGear = newGear;
            // Play gear shift sound
            if (gearShiftAudioSource != null && gearShiftClip != null)
            {
                gearShiftAudioSource.clip = gearShiftClip;
                gearShiftAudioSource.Play();
            }
        }
    }
    private void ApplyDownforce()
    {
        // Apply downforce based on speed
        float downforce = rb.linearVelocity.sqrMagnitude * downforceCoefficient;
        rb.AddForce(-ShortcutPlayer.Transform.up * downforce);
    }
    private void ApplyAntiRoll()
    {
        // Apply anti-roll forces to reduce body roll
        for (int i = 0; i < allWheels.Count; i += 2)
        {
            if (i + 1 < allWheels.Count)
            {
                WheelCollider wheelL = allWheels[i];
                WheelCollider wheelR = allWheels[i + 1];
                WheelHit hitL, hitR;
                bool groundedL = wheelL.GetGroundHit(out hitL);
                bool groundedR = wheelR.GetGroundHit(out hitR);
                float travelL = groundedL ? (-wheelL.transform.InverseTransformPoint(hitL.point).y - wheelL.radius) /
                wheelL.suspensionDistance : 1.0f;
                float travelR = groundedR ? (-wheelR.transform.InverseTransformPoint(hitR.point).y - wheelR.radius) /
                wheelR.suspensionDistance : 1.0f;
                float antiRollForce = (travelL - travelR) * antiRollAmount;
                if (groundedL)
                    rb.AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position);
                if (groundedR)
                    rb.AddForceAtPosition(wheelR.transform.up * antiRollForce, wheelR.transform.position);
            }
        }
    }
    private void UpdateWheelMeshes()
    {
        foreach (WheelCollider wheel in allWheels)
        {
            if (wheelDataMap.ContainsKey(wheel) && wheelDataMap[wheel].wheelMesh != null)
            {
                Vector3 pos;
                Quaternion rot;
                wheel.GetWorldPose(out pos, out rot);
                wheelDataMap[wheel].wheelMesh.position = pos;
                wheelDataMap[wheel].wheelMesh.rotation = rot;
            }
        }
    }
    private void UpdateWheelData()
    {
        burnoutIntensity = 0f;
        foreach (WheelCollider wheel in allWheels)
        {
            if (wheelDataMap.ContainsKey(wheel))
            {
                WheelHit hit;
                WheelData data = wheelDataMap[wheel];
                // Calculate angular velocity
                float currentRotation = wheel.rpm * 6f * Time.fixedDeltaTime;
                data.angularVelocity = (currentRotation - data.lastRotation) / Time.fixedDeltaTime;
                data.lastRotation = currentRotation;
                // Calculate slip ratio
                if (wheel.GetGroundHit(out hit))
                {
                    // Forward slip (acceleration/braking)
                    float forwardSlip = Mathf.Abs(hit.forwardSlip);
                    // Sideways slip (cornering)
                    float sidewaysSlip = Mathf.Abs(hit.sidewaysSlip);
                    // Combined slip
                    data.slipRatio = Mathf.Sqrt(forwardSlip * forwardSlip + sidewaysSlip * sidewaysSlip);
                    // Check if wheel is skidding
                    data.isSkidding = data.slipRatio > skidThreshold;
                    // Calculate burnout intensity for drive wheels
                    if (driveWheels.Contains(wheel) && forwardSlip > 0.5f && currentSpeed < 10f)
                    {
                        burnoutIntensity = Mathf.Max(burnoutIntensity, forwardSlip * burnoutFactor);
                    }
                }
                else
                {
                    data.isSkidding = false;
                    data.slipRatio = 0f;
                }
            }
        }
    }
    private void HandleSounds()
    {
        // Engine sound
        if (engineAudioSource != null && engineRunningClip != null && isEngineRunning)
        {
            // Set engine clip if not already set
            if (engineAudioSource.clip != engineRunningClip)
            {
                engineAudioSource.clip = engineRunningClip;
                engineAudioSource.loop = true;
                engineAudioSource.Play();
            }
            // Adjust pitch based on RPM
            float pitchFactor = Mathf.Lerp(minEnginePitch, maxEnginePitch, (currentRPM - idleRPM) / (maxRPM -
            idleRPM));
            engineAudioSource.pitch = pitchFactor;
            // Adjust volume based on throttle
            float throttleInput = Mathf.Abs(Input.GetAxis("Vertical"));
            engineAudioSource.volume = Mathf.Lerp(0.5f, 1.0f, throttleInput);
        }
        // Skid sound
        if (skidAudioSource != null && skidClip != null)
        {
            bool isAnyWheelSkidding = false;
            float maxSlipRatio = 0f;
            foreach (WheelCollider wheel in allWheels)
            {
                if (wheelDataMap.ContainsKey(wheel) && wheelDataMap[wheel].isSkidding)
                {
                    isAnyWheelSkidding = true;
                    maxSlipRatio = Mathf.Max(maxSlipRatio, wheelDataMap[wheel].slipRatio);
                }
            }
            if (isAnyWheelSkidding)
            {
                if (!skidAudioSource.isPlaying)
                {
                    skidAudioSource.clip = skidClip;
                    skidAudioSource.loop = true;
                    skidAudioSource.Play();
                }
                // Adjust volume based on slip intensity
                skidAudioSource.volume = Mathf.Lerp(0.1f, 1.0f, (maxSlipRatio - skidThreshold) / (1f - skidThreshold));
                // Adjust pitch for burnouts
                if (burnoutIntensity > 0)
                {
                    skidAudioSource.pitch = Mathf.Lerp(1.0f, 1.5f, burnoutIntensity / (burnoutFactor * 2));
                }
                else
                {
                    skidAudioSource.pitch = 1.0f;
                }
            }
            else if (skidAudioSource.isPlaying)
            {
                skidAudioSource.Stop();
            }
        }
    }
    private void HandleParticleEffects()
    {
        foreach (WheelCollider wheel in allWheels)
        {
            if (wheelDataMap.ContainsKey(wheel))
            {
                WheelData data = wheelDataMap[wheel];
                // Smoke particles
                if (data.smoke != null)
                {
                    if (data.isSkidding)
                    {
                        if (!data.smoke.isPlaying)
                        {
                            data.smoke.Play();
                        }
                        // Adjust emission rate based on slip
                        var emission = data.smoke.emission;
                        emission.rateOverTime = Mathf.Lerp(5, 30, (data.slipRatio - skidThreshold) / (1f - skidThreshold));
                        // Make burnout smoke more intense
                        if (burnoutIntensity > 0 && driveWheels.Contains(wheel))
                        {
                            var main = data.smoke.main;
                            main.startSize = Mathf.Lerp(0.5f, 1.5f, burnoutIntensity / burnoutFactor);
                            emission.rateOverTime = Mathf.Lerp(30, 60, burnoutIntensity / burnoutFactor);
                        }
                    }
                    else if (data.smoke.isPlaying)
                    {
                        data.smoke.Stop();
                    }
                }
                // Skid marks
                if (data.skidTrail != null)
                {
                    WheelHit hit;
                    if (wheel.GetGroundHit(out hit))
                    {
                        if (data.isSkidding)
                        {
                            if (!data.skidTrail.emitting)
                            {
                                data.skidTrail.emitting = true;
                            }
                            // Position the trail at the contact point
                            data.skidTrail.transform.position = hit.point + (Vector3.up * 0.01f);
                        }
                        else if (data.skidTrail.emitting)
                        {
                            data.skidTrail.emitting = false;
                        }
                    }
                    else if (data.skidTrail.emitting)
                    {
                        data.skidTrail.emitting = false;
                    }
                }
            }
        }
    }
    private void StartEngine()
    {
        if (!isEngineRunning)
        {
            isEngineRunning = true;
            currentRPM = idleRPM;
            // Play engine start sound
            if (engineAudioSource != null && engineStartClip != null)
            {
                engineAudioSource.clip = engineStartClip;
                engineAudioSource.loop = false;
                engineAudioSource.Play();
                // Schedule the transition to running sound
                this.Character.StartCoroutine(TransitionToRunningSoundAfterDelay(engineStartClip.length));
            }
            else
            {
                TransitionToRunningSound();
            }
        }
    }
    // Coroutine to replace Invoke
    private IEnumerator TransitionToRunningSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TransitionToRunningSound();
    }

    private void TransitionToRunningSound()
    {
        if (engineAudioSource != null && engineRunningClip != null)
        {
            engineAudioSource.clip = engineRunningClip;
            engineAudioSource.loop = true;
            engineAudioSource.Play();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Play crash sound on significant impacts
        if (crashAudioSource != null && crashClip != null && collision.relativeVelocity.magnitude > 5f)
        {
            crashAudioSource.clip = crashClip;
            crashAudioSource.volume = Mathf.Clamp01(collision.relativeVelocity.magnitude / 20f);
            crashAudioSource.Play();
        }
    }
    // Public methods for external control
    public void SetGear(int gear)
    {
        if (gear >= -1 && gear <= numberOfGears)
        {
            // Convert -1 to 0 for reverse
            ShiftGear(gear < 0 ? 0 : gear);
        }
    }
    public float GetSpeed()
    {
        return currentSpeed;
    }
    public float GetRPM()
    {
        return currentRPM;
    }
    public int GetGear()
    {
        return currentGear;
    }
    public float GetEngineLoad()
    {
        return Mathf.Abs(currentMotorTorque / maxMotorTorque);
    }
    public void ToggleEngine()
    {
        if (isEngineRunning)
        {
            isEngineRunning = false;
            if (engineAudioSource != null)
            {
                engineAudioSource.Stop();
            }
        }
        else
        {
            StartEngine();
        }
    }
}