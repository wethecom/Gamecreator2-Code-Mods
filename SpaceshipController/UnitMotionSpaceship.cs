using System;
using UnityEngine;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
[Title("Spaceship Motion")]
[Image(typeof(IconChip), ColorTheme.Type.Blue)]
[Category("Spaceship Motion")]
[Description("Motion system for spaceship-like movement")]
[Serializable]
public class UnitMotionSpaceship : TUnitMotion
{
    private Character character;
    void Start()
    {
        this.character = ShortcutPlayer.Transform.GetComponent<Character>();
        character.Motion.MovementType = Character.MovementType.None;
       
    }

    public override float Acceleration { get; set; } = 0;
    public override float Deceleration { get; set; } = 0;
    public override float LinearSpeed { get; set; } = 0;
    public override float AngularSpeed { get; set; } = 90f;
    public override float GravityUpwards { get; set; } = 0f;
    public override float GravityDownwards { get; set; } = 0f;
    public override float Height { get; set; } = 2f;
    public override float Radius { get; set; } = 0.5f;
    public override float Mass { get; set; } = 100f;
    public override bool UseAcceleration { get; set; } = true;
    public override bool CanJump { get; set; } = false;
    public override int AirJumps { get; set; } = 0;
    public override float JumpForce { get; set; } = 0f;
    public override float JumpCooldown { get; set; } = 0f;
    public override bool DashInAir { get; set; } = false;
    public override int DashInSuccession { get; set; } = 0;
    public override float TerminalVelocity { get; set; } = 100f;
    public override float DashCooldown { get; set; } = 0f;

}

/*using UnityEngine;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using System;

[Title("Spaceship Motion")]
[Image(typeof(IconChip), ColorTheme.Type.Blue)]
[Category("Spaceship Motion")]
[Description("Motion system for spaceship-like movement")]

[Serializable]
public class UnitMotionSpaceship : TUnitMotion
{
    [Header("Movement Settings")]
    [SerializeField] public float thrustForce = 10f;
    [SerializeField] public float strafeForce = 8f;
    [SerializeField] public float verticalForce = 8f;
    [SerializeField] public float rollForce = 100f;
    [SerializeField] public float pitchForce = 100f;
    [SerializeField] public float yawForce = 100f;
    [SerializeField] public float damping = 0.95f;
    [SerializeField] public float scrollSpeed = 5f; // Adjust this value to control roll speed
    [SerializeField] public float liftForce = 10f; // Adjust lift force as needed
    [Header("Weapons")]
    [SerializeField] public Transform[] weaponMounts;
    [SerializeField] public GameObject projectilePrefab;
    [SerializeField] public float projectileSpeed = 50f;
    [SerializeField] public float fireRate = 0.2f;

    [Header("Collision Settings")]
    [SerializeField] public float collisionDamping = 0.7f;
    [SerializeField] public float angularDampingOnCollision = 0.5f;
    [SerializeField] public float bounceForce = 5f;
    [SerializeField] public AudioClip collisionSound;

    [Header("Effects")]
    public ParticleSystem thrusterEffect;
    public ParticleSystem collisionEffect;

    public Rigidbody rb;
    private float nextFireTime;
    private AudioSource audioSource;




    
        [SerializeField] private float m_ThrustSpeed = 10f;
        [SerializeField] private float m_StrafeSpeed = 5f;
        [SerializeField] private float m_VerticalSpeed = 5f;
        [SerializeField] private float m_Acceleration = 5f;
        [SerializeField] private float m_Deceleration = 5f;
    
        [SerializeField] private float m_MaxSpeed = 100f;
        // Implementing abstract properties
        public override float Acceleration { get => m_Acceleration; set => m_Acceleration = value; }
        public override float Deceleration { get => m_Deceleration; set => m_Deceleration = value; }
        public override float LinearSpeed { get => m_ThrustSpeed; set => m_ThrustSpeed = value; }
        public override float AngularSpeed { get; set; } = 90f;
        public override float GravityUpwards { get; set; } = 0f;
        public override float GravityDownwards { get; set; } = 0f;
        public override float Height { get; set; } = 2f;
        public override float Radius { get; set; } = 0.5f;
        public override float Mass { get; set; } = 100f;
        public override bool UseAcceleration { get; set; } = true;
        public override bool CanJump { get; set; } = false;
        public override int AirJumps { get; set; } = 0;
        public override float JumpForce { get; set; } = 0f;
        public override float JumpCooldown { get; set; } = 0f;
        public override bool DashInAir { get; set; } = false;
        public override int DashInSuccession { get; set; } = 0;
        public override float TerminalVelocity { get; set; } = 100f;
    public override float DashCooldown { get; set; } = 0f;
    GameObject player;
    private Character character;
    void Start()
        {
      player = ShortcutPlayer.Instance;
      //  rb = player.GetComponent<Rigidbody>();
            rb.useGravity = false;
            Cursor.lockState = CursorLockMode.Locked;
        // Get the Character component
        this.character = player.GetComponent<Character>();
        if (this.character == null)
        {
            Debug.LogError("No Character component found on this GameObject.");
            return;
        }

        // Disable the Driver and Motion by setting them to null
      

    }

        // Implementing the Update method
        public override void OnUpdate()
        {

        
        }
    
        void FixedUpdate()
        {
          
    }

    void FireWeapons()
    {
       
    }
    void OnCollisionEnter(Collision collision)
    {
      
    }

    // Optional: Add shield/damage system
    public float shields = 100f;
    public float maxShields = 100f;
    public float shieldRechargeRate = 5f;
    public float shieldRechargeDelay = 3f;
    private float lastDamageTime;

   

   

}
*/