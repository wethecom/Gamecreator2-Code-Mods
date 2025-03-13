using System;
using UnityEngine;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;

[Title("Spaceship Driver")]
[Image(typeof(IconCapsuleSolid), ColorTheme.Type.Green)]
[Category("Spaceship Driver")]
[Description("Driver for spaceship-like flying movement")]

[Serializable]
public class UnitDriverSpaceship : TUnitDriver
{
    [Header("Movement Settings")]
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float strafeForce = 8f;
    [SerializeField] private float verticalForce = 8f;
    [SerializeField] private float rollForce = 8f;
    [SerializeField] private float pitchForce = 2f;
    [SerializeField] private float yawForce = 2f;
    [SerializeField] private float damping = 0.95f;

    [Header("Weapons")]
    [SerializeField] private Transform[] weaponMounts;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 50f;
    [SerializeField] private float fireRate = 0.2f;

    private Rigidbody m_Rigidbody;
    private float nextFireTime;

    public override Vector3 WorldMoveDirection => m_Rigidbody != null ? m_Rigidbody.linearVelocity : Vector3.zero;
    public override Vector3 LocalMoveDirection => this.Transform.InverseTransformDirection(this.WorldMoveDirection);
    public override float SkinWidth => 0f;
    public override bool IsGrounded => true;
    public override Vector3 FloorNormal => Vector3.up;

    public override bool Collision { get; set; }
    public override Axonometry Axonometry { get; set; }

    public override void OnStartup(Character character)
    {
        base.OnStartup(character);

        m_Rigidbody = character.GetComponent<Rigidbody>();
        if (m_Rigidbody == null)
        {
            m_Rigidbody = character.gameObject.AddComponent<Rigidbody>();
            m_Rigidbody.useGravity = false;
            m_Rigidbody.constraints = RigidbodyConstraints.None;
        }
    }

    public override void OnUpdate()
    {
        // Handle weapon firing
        HandleWeapons();

        // Apply damping to stabilize movement
        m_Rigidbody.linearVelocity *= damping;
        m_Rigidbody.angularVelocity *= damping;

        // Forward/Backward thrust
        float thrust = Input.GetAxis("Vertical");
        m_Rigidbody.AddForce(this.Transform.forward * thrust * thrustForce);

        // Strafing (left/right)
        float strafe = Input.GetAxis("Horizontal");
        m_Rigidbody.AddForce(this.Transform.right * strafe * strafeForce);

        // Vertical movement (Q/E keys)
        float vertical = 0;
        if (Input.GetKey(KeyCode.E)) vertical += 1;
        if (Input.GetKey(KeyCode.Q)) vertical -= 1;
        m_Rigidbody.AddForce(this.Transform.up * vertical * verticalForce);

        // Roll (mouse scroll)
        float roll = Input.mouseScrollDelta.y;
        m_Rigidbody.AddTorque(this.Transform.forward * roll * rollForce);

        // Pitch (mouse Y)
        float pitch = -Input.GetAxis("Mouse Y");
        m_Rigidbody.AddTorque(this.Transform.right * pitch * pitchForce);

        // Yaw (mouse X)
        float yaw = Input.GetAxis("Mouse X");
        m_Rigidbody.AddTorque(this.Transform.up * yaw * yawForce);
    }

    private void HandleWeapons()
    {
        if (Input.GetButton("Fire1") && Time.time > nextFireTime)
        {
            FireWeapons();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void FireWeapons()
    {
        if (weaponMounts == null || projectilePrefab == null) return;

        foreach (Transform weaponMount in weaponMounts)
        {
            GameObject projectile = UnityEngine.Object.Instantiate(projectilePrefab, weaponMount.position, weaponMount.rotation);
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            if (projectileRb)
            {
                projectileRb.linearVelocity = weaponMount.forward * projectileSpeed;

                // Add ship's velocity to projectile for realistic physics
                projectileRb.linearVelocity += m_Rigidbody.linearVelocity;
            }
            UnityEngine.Object.Destroy(projectile, 5f); // Destroy projectile after 5 seconds
        }
    }

    public override void SetPosition(Vector3 position)
    {
        this.Transform.position = position;
    }

    public override void SetRotation(Quaternion rotation)
    {
        this.Transform.rotation = rotation;
    }

    public override void AddPosition(Vector3 amount)
    {
        this.Transform.position += amount;
    }

    public override void AddRotation(Quaternion amount)
    {
        this.Transform.rotation *= amount;
    }

    public override void ResetVerticalVelocity()
    {
        if (m_Rigidbody != null)
        {
            m_Rigidbody.linearVelocity = new Vector3(m_Rigidbody.linearVelocity.x, 0f, m_Rigidbody.linearVelocity.z);
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
}
