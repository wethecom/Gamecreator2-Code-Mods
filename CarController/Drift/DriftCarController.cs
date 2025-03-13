using UnityEngine;

public class StableDriftCarController : MonoBehaviour
{
    [Header("Wheels")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Wheel Meshes")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    [Header("Car Settings")]
    [Range(1000, 5000)]
    public float motorForce = 2500f;
    [Range(10, 50)]
    public float maxSteerAngle = 30f;
    [Range(0.1f, 1f)]
    public float brakeForce = 0.5f;

    [Header("Drift Settings")]
    [Range(0.1f, 1f)]
    public float tractionControl = 0.8f;
    [Range(0.1f, 1f)]
    public float driftFactor = 0.5f;
    public bool driftEnabled = true;
    [Range(10f, 50f)]
    public float driftSteerMultiplier = 25f;

    [Header("Anti-Tip Settings")]
    [Range(1000, 10000)]
    public float antiRollForce = 5000f;
    public float downforceMultiplier = 1.0f;

    [Header("Anti-Jitter Settings")]
    public float wheelDampingRate = 0.25f;
    public int solverIterationCount = 10;
    public float interpolationFactor = 0.5f;
    public bool useInterpolation = true;

    private Rigidbody rb;
    private float slipAllowance = 0.3f;
    private bool isDrifting = false;
    private float currentSpeed;
    private Vector3 prevPosition;
    private Quaternion prevRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lower center of mass to prevent tipping
        rb.centerOfMass = new Vector3(0, -0.8f, 0);

        // Increase mass for better stability
        if (rb.mass < 1500)
            rb.mass = 1500;

        // Increase solver iterations to reduce jitter
        rb.solverIterations = solverIterationCount;
        rb.solverVelocityIterations = solverIterationCount;

        // Reduce angular drag to prevent jittering
        rb.angularDamping = 0.05f;

        // Configure wheel friction
        ConfigureWheelFriction();

        // Configure wheel suspension
        ConfigureWheelSuspension();

        // Store initial position for interpolation
        prevPosition = transform.position;
        prevRotation = transform.rotation;
    }

    void ConfigureWheelFriction()
    {
        WheelCollider[] wheels = { frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel };

        foreach (WheelCollider wheel in wheels)
        {
            WheelFrictionCurve fwdFriction = wheel.forwardFriction;
            fwdFriction.stiffness = 1.0f;
            fwdFriction.extremumSlip = 0.4f;
            fwdFriction.extremumValue = 1.0f;
            fwdFriction.asymptoteSlip = 0.8f;
            fwdFriction.asymptoteValue = 0.8f;
            wheel.forwardFriction = fwdFriction;

            WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
            sideFriction.stiffness = 1.0f;
            sideFriction.extremumSlip = 0.25f;
            sideFriction.extremumValue = 1.0f;
            sideFriction.asymptoteSlip = 0.5f;
            sideFriction.asymptoteValue = 0.8f;
            wheel.sidewaysFriction = sideFriction;
        }
    }

    void ConfigureWheelSuspension()
    {
        WheelCollider[] wheels = { frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel };

        foreach (WheelCollider wheel in wheels)
        {
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = 35000f;
            spring.damper = 4500f;
            spring.targetPosition = 0.5f;

            wheel.suspensionSpring = spring;
            wheel.suspensionDistance = 0.2f;
            wheel.forceAppPointDistance = 0.1f;  // Lower force application point

            // Set wheel damping rate to reduce jitter
            wheel.wheelDampingRate = wheelDampingRate;

            // Increase wheel mass and radius for better stability
            wheel.mass = 40f;
            if (wheel.radius < 0.4f)
                wheel.radius = 0.4f;
        }
    }

    void FixedUpdate()
    {
        // Store current position and rotation for interpolation
        prevPosition = transform.position;
        prevRotation = transform.rotation;

        float acceleration = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");
        float brake = Input.GetKey(KeyCode.Space) ? brakeForce * 10000f : 0f;

        // Calculate current speed
        currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert to km/h

        // Check for drift input (e.g., holding handbrake)
        bool handbrakeInput = Input.GetKey(KeyCode.LeftShift);

        // Apply motor torque to rear wheels with improved acceleration
        float motorTorque = acceleration * motorForce;
        rearLeftWheel.motorTorque = motorTorque;
        rearRightWheel.motorTorque = motorTorque;

        // Progressive steering (less at high speeds)
        float speedFactor = Mathf.Clamp01(currentSpeed / 100f);
        float currentSteerAngle = maxSteerAngle * (1f - (speedFactor * 0.5f));

        // Apply steering to front wheels with smoothing
        float targetSteerAngle = steering * currentSteerAngle;
        frontLeftWheel.steerAngle = Mathf.Lerp(frontLeftWheel.steerAngle, targetSteerAngle, 0.2f);
        frontRightWheel.steerAngle = Mathf.Lerp(frontRightWheel.steerAngle, targetSteerAngle, 0.2f);

        // Apply braking
        ApplyBraking(brake);

        // Handle drifting
        if (driftEnabled)
        {
            HandleDrift(handbrakeInput, steering);
        }

        // Apply traction control
        ApplyTractionControl();

        // Apply anti-roll forces to prevent tipping
        ApplyAntiRoll();

        // Apply downforce to prevent tipping at high speeds
        ApplyDownforce();

        // Update wheel meshes
        UpdateWheelPoses();

        // Apply velocity damping to reduce jitter
        ApplyVelocityDamping();
    }

    void Update()
    {
        // Apply visual interpolation in Update to reduce jitter
        if (useInterpolation && Time.timeScale > 0)
        {
            // Only interpolate if we're moving at a reasonable speed
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                // Interpolate position and rotation for smoother visuals
                transform.position = Vector3.Lerp(prevPosition, transform.position, interpolationFactor);
                transform.rotation = Quaternion.Slerp(prevRotation, transform.rotation, interpolationFactor);
            }
        }
    }

    void ApplyVelocityDamping()
    {
        // Apply slight damping to angular velocity to reduce jitter
        if (rb.angularVelocity.magnitude > 0.5f)
        {
            rb.angularVelocity *= 0.95f;
        }

        // Eliminate very small movements that can cause jitter
        if (rb.linearVelocity.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void HandleDrift(bool handbrakeInput, float steering)
    {
        // Get wheel slip information
        WheelHit hit;
        float slipSum = 0;
        int wheelCount = 0;

        if (rearLeftWheel.GetGroundHit(out hit))
        {
            slipSum += Mathf.Abs(hit.sidewaysSlip);
            wheelCount++;
        }

        if (rearRightWheel.GetGroundHit(out hit))
        {
            slipSum += Mathf.Abs(hit.sidewaysSlip);
            wheelCount++;
        }

        float averageSlip = wheelCount > 0 ? slipSum / wheelCount : 0;

        // Determine if we're drifting
        isDrifting = (averageSlip > slipAllowance || handbrakeInput) && currentSpeed > 20f;

        if (isDrifting)
        {
            // Reduce rear wheel friction for drifting
            AdjustWheelFriction(rearLeftWheel, driftFactor);
            AdjustWheelFriction(rearRightWheel, driftFactor);

            // Apply counter-steering assistance
            if (Mathf.Abs(steering) > 0.1f && rb.linearVelocity.magnitude > 5f)
            {
                float driftSteerForce = steering * driftSteerMultiplier;
                rb.AddForce(transform.right * driftSteerForce, ForceMode.Force);
            }

            // Apply handbrake if requested
            if (handbrakeInput)
            {
                rearLeftWheel.brakeTorque = brakeForce * 5000f;
                rearRightWheel.brakeTorque = brakeForce * 5000f;
            }
        }
        else
        {
            // Normal driving friction
            AdjustWheelFriction(rearLeftWheel, 1.0f);
            AdjustWheelFriction(rearRightWheel, 1.0f);
        }
    }

    void AdjustWheelFriction(WheelCollider wheel, float stiffnessFactor)
    {
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        sidewaysFriction.stiffness = stiffnessFactor;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    void ApplyTractionControl()
    {
        WheelHit wheelHit;

        // Apply traction control to rear wheels
        if (rearLeftWheel.GetGroundHit(out wheelHit))
        {
            AdjustTorqueForWheel(rearLeftWheel, wheelHit.forwardSlip);
        }

        if (rearRightWheel.GetGroundHit(out wheelHit))
        {
            AdjustTorqueForWheel(rearRightWheel, wheelHit.forwardSlip);
        }
    }

    void AdjustTorqueForWheel(WheelCollider wheel, float forwardSlip)
    {
        if (forwardSlip >= slipAllowance && !isDrifting)
        {
            // Reduce torque to regain traction
            wheel.motorTorque *= tractionControl;
        }
    }

    void ApplyBraking(float brakeForce)
    {
        frontLeftWheel.brakeTorque = brakeForce;
        frontRightWheel.brakeTorque = brakeForce;

        // Only apply to rear if not drifting
        if (!isDrifting)
        {
            rearLeftWheel.brakeTorque = brakeForce;
            rearRightWheel.brakeTorque = brakeForce;
        }
    }

    void ApplyAntiRoll()
    {
        // Front anti-roll bar
        ApplyAntiRollToAxle(frontLeftWheel, frontRightWheel);

        // Rear anti-roll bar
        ApplyAntiRollToAxle(rearLeftWheel, rearRightWheel);
    }

    void ApplyAntiRollToAxle(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit leftWheelHit, rightWheelHit;
        bool groundedLeft = leftWheel.GetGroundHit(out leftWheelHit);
        bool groundedRight = rightWheel.GetGroundHit(out rightWheelHit);

        float leftTravel = groundedLeft ? (-leftWheel.transform.InverseTransformPoint(leftWheelHit.point).y - leftWheel.radius) / leftWheel.suspensionDistance : 1.0f;
        float rightTravel = groundedRight ? (-rightWheel.transform.InverseTransformPoint(rightWheelHit.point).y - rightWheel.radius) / rightWheel.suspensionDistance : 1.0f;

        float antiRollForceAmount = (leftTravel - rightTravel) * antiRollForce;

        if (groundedLeft)
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForceAmount, leftWheel.transform.position);

        if (groundedRight)
            rb.AddForceAtPosition(rightWheel.transform.up * antiRollForceAmount, rightWheel.transform.position);
    }

    void ApplyDownforce()
    {
        // Apply downforce based on speed
        float downforce = rb.mass * downforceMultiplier * (currentSpeed / 100f);
        rb.AddForce(-transform.up * downforce);
    }

    void UpdateWheelPoses()
    {
        UpdateWheelPose(frontLeftWheel, frontLeftTransform);
        UpdateWheelPose(frontRightWheel, frontRightTransform);
        UpdateWheelPose(rearLeftWheel, rearLeftTransform);
        UpdateWheelPose(rearRightWheel, rearRightTransform);
    }

    void UpdateWheelPose(WheelCollider collider, Transform transform)
    {
        Vector3 position;
        Quaternion rotation;
        collider.GetGroundHit(out WheelHit hit);

        // Get wheel pose
        collider.GetWorldPose(out position, out rotation);

        // Apply smoothing to wheel visual position
        if (useInterpolation)
        {
            transform.position = Vector3.Lerp(transform.position, position, interpolationFactor);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, interpolationFactor);
        }
        else
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}