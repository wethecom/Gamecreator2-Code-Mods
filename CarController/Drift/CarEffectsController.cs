using UnityEngine;
using System.Collections.Generic;

public class CarEffectsController : MonoBehaviour
{
    [Header("References")]
    public StableDriftCarController carController;
    public CarSoundController soundController;
    
    [Header("Brake Lights")]
    public Light[] brakeLights;
    public float brakeIntensity = 3.0f;
    public Color brakeLightColor = Color.red;
    
    [Header("Tire Marks")]
    public bool enableTireMarks = true;
    public Material driftMarkMaterial;
    public Material burnoutMarkMaterial;
    public Material brakeMarkMaterial;
    [Range(0.05f, 0.5f)]
    public float markWidth = 0.2f;
    [Range(0.1f, 1.0f)]
    public float driftMarkThreshold = 0.3f;
    [Range(0.1f, 1.0f)]
    public float burnoutMarkThreshold = 0.5f;
    [Range(0.1f, 1.0f)]
    public float brakeMarkThreshold = 0.7f;
    
    [Header("Smoke Effects")]
    public bool enableSmoke = true;
    public ParticleSystem driftSmokeTemplate;
    public ParticleSystem burnoutSmokeTemplate;
    public ParticleSystem brakeSmokeTemplate;
    [Range(0.1f, 1.0f)]
    public float driftSmokeThreshold = 0.3f;
    [Range(0.1f, 1.0f)]
    public float burnoutSmokeThreshold = 0.4f;
    [Range(0.1f, 1.0f)]
    public float brakeSmokeThreshold = 0.8f;
    
    [Header("Exhaust Effects")]
    public bool enableExhaust = true;
    public ParticleSystem[] exhaustParticles;
    [Range(0.1f, 10.0f)]
    public float exhaustMultiplier = 1.0f;
    
    // Private variables
    private Rigidbody rb;
    private WheelCollider[] wheelColliders = new WheelCollider[4];
    private Transform[] wheelTransforms = new Transform[4];
    private ParticleSystem[] driftSmokeParticles = new ParticleSystem[4];
    private ParticleSystem[] burnoutSmokeParticles = new ParticleSystem[4];
    private ParticleSystem[] brakeSmokeParticles = new ParticleSystem[4];
    private TrailRenderer[] tireMarks = new TrailRenderer[4];
    private float currentSpeed;
    private bool isBraking;
    private bool isDrifting;
    private bool isBurningOut;
    private float brakeIntensityValue;
    private float[] wheelSlipAmount = new float[4];
    private float[] wheelForwardSlip = new float[4];
    private float previousSpeed;
    private float deceleration;
    
    void Start()
    {
        rb = carController.GetComponent<Rigidbody>();
        
        // Get wheel colliders and transforms
        wheelColliders[0] = carController.frontLeftWheel;
        wheelColliders[1] = carController.frontRightWheel;
        wheelColliders[2] = carController.rearLeftWheel;
        wheelColliders[3] = carController.rearRightWheel;
        
        wheelTransforms[0] = carController.frontLeftTransform;
        wheelTransforms[1] = carController.frontRightTransform;
        wheelTransforms[2] = carController.rearLeftTransform;
        wheelTransforms[3] = carController.rearRightTransform;
        
        // Initialize brake lights
        if (brakeLights != null && brakeLights.Length > 0)
        {
            foreach (Light light in brakeLights)
            {
                light.color = brakeLightColor;
                light.intensity = 0;
            }
        }
        
        // Initialize tire marks
        if (enableTireMarks)
        {
            InitializeTireMarks();
        }
        
        // Initialize smoke effects
        if (enableSmoke)
        {
            InitializeSmokeEffects();
        }
    }
    
    void InitializeTireMarks()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject trailObj = new GameObject("TireMark_" + i);
            trailObj.transform.parent = wheelTransforms[i];
            trailObj.transform.localPosition = new Vector3(0, -wheelColliders[i].radius + 0.01f, 0);
            trailObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            
            TrailRenderer trail = trailObj.AddComponent<TrailRenderer>();
            trail.startWidth = markWidth;
            trail.endWidth = markWidth;
            trail.time = 10f; // How long the trail lasts in seconds
            trail.material = driftMarkMaterial;
            trail.emitting = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            
            // Create a gradient for fading out
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trail.colorGradient = gradient;
            
            tireMarks[i] = trail;
        }
    }
    
    void InitializeSmokeEffects()
    {
        // Create smoke particle systems for each wheel
        for (int i = 0; i < 4; i++)
        {
            // Drift smoke
            if (driftSmokeTemplate != null)
            {
                driftSmokeParticles[i] = Instantiate(driftSmokeTemplate, wheelTransforms[i]);
                driftSmokeParticles[i].transform.localPosition = new Vector3(0, -wheelColliders[i].radius * 0.5f, 0);
                driftSmokeParticles[i].Stop();
            }
            
            // Burnout smoke
            if (burnoutSmokeTemplate != null)
            {
                burnoutSmokeParticles[i] = Instantiate(burnoutSmokeTemplate, wheelTransforms[i]);
                burnoutSmokeParticles[i].transform.localPosition = new Vector3(0, -wheelColliders[i].radius * 0.5f, 0);
                burnoutSmokeParticles[i].Stop();
            }
            
            // Brake smoke
            if (brakeSmokeTemplate != null)
            {
                brakeSmokeParticles[i] = Instantiate(brakeSmokeTemplate, wheelTransforms[i]);
                brakeSmokeParticles[i].transform.localPosition = new Vector3(0, -wheelColliders[i].radius * 0.5f, 0);
                brakeSmokeParticles[i].Stop();
            }
        }
    }
    
    void Update()
    {
        // Get current speed
        previousSpeed = currentSpeed;
        currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert to km/h
        
        // Calculate deceleration
        deceleration = (previousSpeed - currentSpeed) / Time.deltaTime;
        
        // Check vehicle state
        CheckVehicleState();
        
        // Update visual effects
        UpdateBrakeLights();
        UpdateTireMarks();
        UpdateSmokeEffects();
        UpdateExhaustEffects();
    }
    
    void CheckVehicleState()
    {
        isDrifting = false;
        isBurningOut = false;
        isBraking = false;
        
        // Check brake input
        bool brakeInput = Input.GetKey(KeyCode.Space);
        bool isDecelerating = deceleration > 5f && Input.GetAxis("Vertical") <= 0;
        isBraking = (brakeInput || isDecelerating) && currentSpeed > 5f;
        
        // Calculate brake intensity
        if (isBraking)
        {
            if (brakeInput)
            {
                brakeIntensityValue = Mathf.Lerp(brakeIntensityValue, 1f, Time.deltaTime * 3f);
            }
            else
            {
                brakeIntensityValue = Mathf.Lerp(brakeIntensityValue, Mathf.Clamp01(deceleration / 20f), Time.deltaTime * 3f);
            }
        }
        else
        {
            brakeIntensityValue = Mathf.Lerp(brakeIntensityValue, 0f, Time.deltaTime * 5f);
        }
        
        // Check wheel slip for each wheel
        for (int i = 0; i < 4; i++)
        {
            WheelHit hit;
            if (wheelColliders[i].GetGroundHit(out hit))
            {
                wheelSlipAmount[i] = Mathf.Abs(hit.sidewaysSlip);
                wheelForwardSlip[i] = Mathf.Abs(hit.forwardSlip);
                
                // Check for drifting (sideways slip)
                if (wheelSlipAmount[i] > driftMarkThreshold && currentSpeed > 20f)
                {
                    isDrifting = true;
                }
                
                // Check for burnout (forward slip)
                if (wheelForwardSlip[i] > burnoutMarkThreshold && currentSpeed < 20f && 
                    Mathf.Abs(Input.GetAxis("Vertical")) > 0.5f)
                {
                    isBurningOut = true;
                }
            }
            else
            {
                wheelSlipAmount[i] = 0;
                wheelForwardSlip[i] = 0;
            }
        }
        
        // Alternative drift detection using handbrake
        bool handbrakeInput = Input.GetKey(KeyCode.LeftShift);
        if (handbrakeInput && currentSpeed > 20f && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
        {
            isDrifting = true;
        }
    }
    
    void UpdateBrakeLights()
    {
        if (brakeLights == null || brakeLights.Length == 0) return;
        
        foreach (Light light in brakeLights)
        {
            // Set brake light intensity based on brake input
            light.intensity = Mathf.Lerp(light.intensity, isBraking ? brakeIntensity : 0, Time.deltaTime * 10f);
        }
    }
    
    void UpdateTireMarks()
    {
        if (!enableTireMarks) return;
        
        for (int i = 0; i < 4; i++)
        {
            bool shouldEmitMark = false;
            
            // Determine which type of mark to emit
            if (isBraking && brakeIntensityValue > brakeMarkThreshold && currentSpeed > 10f)
            {
                // Brake marks
                tireMarks[i].material = brakeMarkMaterial;
                shouldEmitMark = true;
            }
            else if (isDrifting && wheelSlipAmount[i] > driftMarkThreshold)
            {
                // Drift marks
                tireMarks[i].material = driftMarkMaterial;
                shouldEmitMark = true;
            }
            else if (isBurningOut && wheelForwardSlip[i] > burnoutMarkThreshold && (i == 2 || i == 3)) // Only rear wheels for burnout
            {
                // Burnout marks
                tireMarks[i].material = burnoutMarkMaterial;
                shouldEmitMark = true;
            }
            
            // Check if wheel is grounded
            WheelHit hit;
            bool isGrounded = wheelColliders[i].GetGroundHit(out hit);
            
            // Only emit marks if the wheel is on the ground
            tireMarks[i].emitting = shouldEmitMark && isGrounded;
            
            // Update mark position to follow the wheel contact point
            if (isGrounded)
            {
                tireMarks[i].transform.position = hit.point + (Vector3.up * 0.01f);
            }
        }
    }
    
    void UpdateSmokeEffects()
    {
        if (!enableSmoke) return;
        
        for (int i = 0; i < 4; i++)
        {
            // Check if wheel is grounded
            WheelHit hit;
            bool isGrounded = wheelColliders[i].GetGroundHit(out hit);
            
            // Drift smoke
            if (driftSmokeParticles[i] != null)
            {
                bool shouldEmitDriftSmoke = isDrifting && wheelSlipAmount[i] > driftSmokeThreshold && isGrounded;
                
                var driftEmission = driftSmokeParticles[i].emission;
                if (shouldEmitDriftSmoke)
                {
                    if (!driftSmokeParticles[i].isPlaying)
                        driftSmokeParticles[i].Play();
                    
                    // Adjust emission rate based on slip amount
                    float emissionRate = Mathf.Lerp(5f, 30f, Mathf.Clamp01((wheelSlipAmount[i] - driftSmokeThreshold) / 0.5f));
                    driftEmission.rateOverTime = emissionRate;
                }
                else
                {
                    if (driftSmokeParticles[i].isPlaying)
                        driftSmokeParticles[i].Stop();
                }
            }
            
            // Burnout smoke
            if (burnoutSmokeParticles[i] != null)
            {
                bool shouldEmitBurnoutSmoke = isBurningOut && wheelForwardSlip[i] > burnoutSmokeThreshold && isGrounded && (i == 2 || i == 3);
                
                var burnoutEmission = burnoutSmokeParticles[i].emission;
                if (shouldEmitBurnoutSmoke)
                {
                    if (!burnoutSmokeParticles[i].isPlaying)
                        burnoutSmokeParticles[i].Play();
                    
                    // Adjust emission rate based on slip amount
                    float emissionRate = Mathf.Lerp(10f, 50f, Mathf.Clamp01((wheelForwardSlip[i] - burnoutSmokeThreshold) / 0.5f));
                    burnoutEmission.rateOverTime = emissionRate;
                }
                else
                {
                    if (burnoutSmokeParticles[i].isPlaying)
                        burnoutSmokeParticles[i].Stop();
                }
            }
            
            // Brake smoke
            if (brakeSmokeParticles[i] != null)
            {
                bool shouldEmitBrakeSmoke = isBraking && brakeIntensityValue > brakeSmokeThreshold && currentSpeed > 50f && isGrounded;
                
                var brakeEmission = brakeSmokeParticles[i].emission;
                if (shouldEmitBrakeSmoke)
                {
                    if (!brakeSmokeParticles[i].isPlaying)
                        brakeSmokeParticles[i].Play();
                    
                    // Adjust emission rate based on brake intensity and speed
                    float emissionRate = Mathf.Lerp(5f, 20f, brakeIntensityValue * Mathf.Clamp01(currentSpeed / 100f));
                    brakeEmission.rateOverTime = emissionRate;
                }
                else
                {
                    if (brakeSmokeParticles[i].isPlaying)
                        brakeSmokeParticles[i].Stop();
                }
            }
        }
    }
    
    void UpdateExhaustEffects()
    {
        if (!enableExhaust || exhaustParticles == null || exhaustParticles.Length == 0) return;
        
        // Get acceleration input
        float accelerationInput = Mathf.Abs(Input.GetAxis("Vertical"));
        
        foreach (ParticleSystem exhaust in exhaustParticles)
        {
            if (exhaust == null) continue;
            
            var emission = exhaust.emission;
            
            // Idle exhaust
            float baseRate = 5f;
            
            // Acceleration exhaust
            float accelRate = accelerationInput * 30f * exhaustMultiplier;
            
            // Rev exhaust (when stationary)
            float revRate = 0f;
            if (currentSpeed < 5f && accelerationInput > 0.5f)
            {
                revRate = accelerationInput * 50f * exhaustMultiplier;
            }
            
            // Set emission rate
            emission.rateOverTime = baseRate + accelRate + revRate;
            
            // Adjust particle size based on acceleration
            var main = exhaust.main;
            main.startSize = Mathf.Lerp(0.1f, 0.3f, accelerationInput) * exhaustMultiplier;
            
            // Adjust particle speed based on acceleration
            main.startSpeed = Mathf.Lerp(1f, 5f, accelerationInput) * exhaustMultiplier;
        }
    }
    
    // Helper method to manually set brake lights (for testing)
    public void SetBrakeLights(bool enabled)
    {
        if (brakeLights == null || brakeLights.Length == 0) return;
        
        foreach (Light light in brakeLights)
        {
            light.intensity = enabled ? brakeIntensity : 0;
        }
    }
}