using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarSoundController : MonoBehaviour
{
    [Header("References")]
    // Reference to the car controller script
    public StableDriftCarController carController;

    [Header("Audio Sources")]
    // Main engine audio source (attached to this GameObject)
    private AudioSource engineAudioSource;
    // Additional audio sources for tire effects
    public AudioSource tireScreechAudioSource;
    public AudioSource burnoutAudioSource;
    public AudioSource brakeAudioSource;

    [Header("Audio Clips")]
    public AudioClip engineSound;
    public AudioClip tireScreechSound;
    public AudioClip burnoutSound;
    public AudioClip brakeSound;

    [Header("Engine Sound Settings")]
    [Range(0.1f, 3.0f)]
    public float minEnginePitch = 0.5f;
    [Range(0.5f, 5.0f)]
    public float maxEnginePitch = 2.0f;
    [Range(0.0f, 1.0f)]
    public float engineVolume = 0.7f;

    [Header("Tire Screech Settings")]
    [Range(0.5f, 2.0f)]
    public float minScreechPitch = 0.8f;
    [Range(1.0f, 3.0f)]
    public float maxScreechPitch = 1.2f;
    [Range(0.0f, 1.0f)]
    public float screechVolume = 0.5f;
    [Range(0.1f, 1.0f)]
    public float driftSlipThreshold = 0.2f;

    [Header("Burnout Settings")]
    [Range(0.5f, 2.0f)]
    public float minBurnoutPitch = 0.9f;
    [Range(1.0f, 3.0f)]
    public float maxBurnoutPitch = 1.5f;
    [Range(0.0f, 1.0f)]
    public float burnoutVolume = 0.6f;
    [Range(0.1f, 1.0f)]
    public float burnoutSlipThreshold = 0.5f;

    [Header("Brake Sound Settings")]
    [Range(0.5f, 2.0f)]
    public float minBrakePitch = 0.8f;
    [Range(1.0f, 3.0f)]
    public float maxBrakePitch = 1.2f;
    [Range(0.0f, 1.0f)]
    public float brakeVolume = 0.5f;
    public float brakeThreshold = 0.3f; // Minimum brake input to trigger sound

    [Header("Sound Thresholds")]
    public float minSpeedForSound = 5f;

    // Private variables for tracking state
    private float currentSpeed;
    private float currentRPM;
    private bool isDrifting;
    private bool isBurningOut;
    private bool isBraking;
    private float brakeIntensity;
    private Rigidbody rb;
    private WheelCollider[] wheelColliders = new WheelCollider[4];
    private float maxSidewaysSlip = 0f;
    private float previousSpeed = 0f;
    private float deceleration = 0f;

    void Start()
    {
        // Get the rigidbody from the car
        rb = carController.GetComponent<Rigidbody>();

        // Setup engine audio source
        engineAudioSource = GetComponent<AudioSource>();
        engineAudioSource.clip = engineSound;
        engineAudioSource.loop = true;
        engineAudioSource.volume = engineVolume;
        engineAudioSource.Play();

        // Setup tire screech audio source if not assigned
        if (tireScreechAudioSource == null)
        {
            GameObject screechObj = new GameObject("TireScreechAudio");
            screechObj.transform.parent = transform;
            screechObj.transform.localPosition = Vector3.zero;
            tireScreechAudioSource = screechObj.AddComponent<AudioSource>();
            tireScreechAudioSource.spatialBlend = 0.5f;
        }
        tireScreechAudioSource.clip = tireScreechSound;
        tireScreechAudioSource.loop = true;
        tireScreechAudioSource.volume = 0;
        tireScreechAudioSource.Play();

        // Setup burnout audio source if not assigned
        if (burnoutAudioSource == null)
        {
            GameObject burnoutObj = new GameObject("BurnoutAudio");
            burnoutObj.transform.parent = transform;
            burnoutObj.transform.localPosition = Vector3.zero;
            burnoutAudioSource = burnoutObj.AddComponent<AudioSource>();
            burnoutAudioSource.spatialBlend = 0.5f;
        }
        burnoutAudioSource.clip = burnoutSound;
        burnoutAudioSource.loop = true;
        burnoutAudioSource.volume = 0;
        burnoutAudioSource.Play();

        // Setup brake audio source if not assigned
        if (brakeAudioSource == null)
        {
            GameObject brakeObj = new GameObject("BrakeAudio");
            brakeObj.transform.parent = transform;
            brakeObj.transform.localPosition = Vector3.zero;
            brakeAudioSource = brakeObj.AddComponent<AudioSource>();
            brakeAudioSource.spatialBlend = 0.5f;
        }
        brakeAudioSource.clip = brakeSound;
        brakeAudioSource.loop = true;
        brakeAudioSource.volume = 0;
        brakeAudioSource.Play();

        // Get wheel colliders from car controller
        wheelColliders[0] = carController.frontLeftWheel;
        wheelColliders[1] = carController.frontRightWheel;
        wheelColliders[2] = carController.rearLeftWheel;
        wheelColliders[3] = carController.rearRightWheel;
    }

    void Update()
    {
        // Get current speed from rigidbody
        previousSpeed = currentSpeed;
        currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert to km/h

        // Calculate deceleration (negative acceleration)
        deceleration = (previousSpeed - currentSpeed) / Time.deltaTime;

        // Calculate a simulated RPM based on speed and acceleration input
        float accelerationInput = Input.GetAxis("Vertical");
        float targetRPM = Mathf.Abs(accelerationInput) * (currentSpeed / 100f);

        // Add idle RPM and smooth the RPM changes
        currentRPM = Mathf.Lerp(currentRPM, 0.2f + targetRPM, Time.deltaTime * 2f);

        // Update engine sound
        UpdateEngineSound();

        // Check for drifting, burnout, and braking
        CheckTireEffects();
        CheckBraking();

        // Update all sound effects
        UpdateTireEffectSounds();
        UpdateBrakeSound();
    }

    void UpdateEngineSound()
    {
        // Calculate engine pitch based on RPM
        float enginePitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, currentRPM);

        // Apply pitch to engine sound
        engineAudioSource.pitch = enginePitch;

        // Adjust volume based on speed (quieter when stationary)
        engineAudioSource.volume = Mathf.Lerp(engineVolume * 0.5f, engineVolume,
                                             Mathf.Clamp01(currentSpeed / minSpeedForSound));
    }

    void CheckTireEffects()
    {
        isDrifting = false;
        isBurningOut = false;
        maxSidewaysSlip = 0f;

        // Check all wheels for slip
        foreach (WheelCollider wheel in wheelColliders)
        {
            WheelHit hit;
            if (wheel.GetGroundHit(out hit))
            {
                // Check for sideways slip (drifting)
                float sidewaysSlip = Mathf.Abs(hit.sidewaysSlip);
                maxSidewaysSlip = Mathf.Max(maxSidewaysSlip, sidewaysSlip);

                if (sidewaysSlip > driftSlipThreshold && currentSpeed > minSpeedForSound)
                {
                    isDrifting = true;
                }

                // Check for forward slip (burnout)
                float forwardSlip = Mathf.Abs(hit.forwardSlip);
                if (forwardSlip > burnoutSlipThreshold && currentSpeed < 20f &&
                    Mathf.Abs(Input.GetAxis("Vertical")) > 0.5f)
                {
                    isBurningOut = true;
                }
            }
        }

        // Alternative drift detection using handbrake input
        bool handbrakeInput = Input.GetKey(KeyCode.LeftShift);
        if (handbrakeInput && currentSpeed > minSpeedForSound && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
        {
            isDrifting = true;
        }
    }

    void CheckBraking()
    {
        // Check for brake input
        bool brakeInput = Input.GetKey(KeyCode.Space);

        // Check for deceleration without gas
        bool isDecelerating = deceleration > 5f && Input.GetAxis("Vertical") <= 0;

        // Determine if we're braking
        isBraking = (brakeInput || isDecelerating) && currentSpeed > minSpeedForSound;

        // Calculate brake intensity
        if (isBraking)
        {
            if (brakeInput)
            {
                // Direct brake input
                brakeIntensity = Mathf.Lerp(brakeIntensity, 1f, Time.deltaTime * 3f);
            }
            else
            {
                // Deceleration-based braking
                brakeIntensity = Mathf.Lerp(brakeIntensity,
                                           Mathf.Clamp01(deceleration / 20f),
                                           Time.deltaTime * 3f);
            }
        }
        else
        {
            brakeIntensity = Mathf.Lerp(brakeIntensity, 0f, Time.deltaTime * 5f);
        }
    }

    void UpdateTireEffectSounds()
    {
        // Handle tire screech sound (drifting)
        if (isDrifting)
        {
            // Calculate pitch based on speed and slip amount
            float slipFactor = Mathf.Clamp01(maxSidewaysSlip / 1.0f);
            float speedFactor = Mathf.Clamp01(currentSpeed / 100f);
            float screechPitch = Mathf.Lerp(minScreechPitch, maxScreechPitch,
                                           (slipFactor + speedFactor) * 0.5f);

            // Calculate volume based on slip intensity
            float targetVolume = screechVolume * Mathf.Clamp01((maxSidewaysSlip - driftSlipThreshold) / 0.5f);
            targetVolume = Mathf.Max(targetVolume, screechVolume * 0.5f); // Ensure minimum volume when drifting

            // Fade in the sound
            tireScreechAudioSource.volume = Mathf.Lerp(tireScreechAudioSource.volume, targetVolume, Time.deltaTime * 10f);
            tireScreechAudioSource.pitch = screechPitch;
        }
        else
        {
            // Fade out the sound
            tireScreechAudioSource.volume = Mathf.Lerp(tireScreechAudioSource.volume, 0f, Time.deltaTime * 10f);
        }

        // Handle burnout sound
        if (isBurningOut)
        {
            // Calculate pitch based on input intensity
            float burnoutPitch = Mathf.Lerp(minBurnoutPitch, maxBurnoutPitch,
                                           Mathf.Abs(Input.GetAxis("Vertical")));

            // Fade in the sound
            burnoutAudioSource.volume = Mathf.Lerp(burnoutAudioSource.volume, burnoutVolume, Time.deltaTime * 5f);
            burnoutAudioSource.pitch = burnoutPitch;
        }
        else
        {
            // Fade out the sound
            burnoutAudioSource.volume = Mathf.Lerp(burnoutAudioSource.volume, 0f, Time.deltaTime * 5f);
        }
    }

    void UpdateBrakeSound()
    {
        if (isBraking && brakeIntensity > brakeThreshold)
        {
            // Calculate brake sound pitch based on speed and intensity
            float speedFactor = Mathf.Clamp01(currentSpeed / 100f);
            float brakePitch = Mathf.Lerp(minBrakePitch, maxBrakePitch,
                                         speedFactor * brakeIntensity);

            // Calculate volume based on brake intensity and speed
            float targetVolume = brakeVolume * brakeIntensity * Mathf.Clamp01(currentSpeed / 20f);

            // Fade in the sound
            brakeAudioSource.volume = Mathf.Lerp(brakeAudioSource.volume, targetVolume, Time.deltaTime * 8f);
            brakeAudioSource.pitch = brakePitch;
        }
        else
        {
            // Fade out the sound
            brakeAudioSource.volume = Mathf.Lerp(brakeAudioSource.volume, 0f, Time.deltaTime * 5f);
        }
    }

    // Helper methods for testing
    public void ForceTireScreech(bool enabled)
    {
        if (enabled)
        {
            tireScreechAudioSource.volume = screechVolume;
            tireScreechAudioSource.pitch = minScreechPitch;
        }
        else
        {
            tireScreechAudioSource.volume = 0f;
        }
    }

    public void ForceBrakeSound(bool enabled, float intensity = 1.0f)
    {
        if (enabled)
        {
            brakeAudioSource.volume = brakeVolume * intensity;
            brakeAudioSource.pitch = Mathf.Lerp(minBrakePitch, maxBrakePitch, intensity);
        }
        else
        {
            brakeAudioSource.volume = 0f;
        }
    }
}