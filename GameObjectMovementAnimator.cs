using UnityEngine;
using GameCreator.Runtime.Characters;

public class GameObjectMovementAnimator : MonoBehaviour
{
    private Character m_Character;
    private Animator m_Animator;

    private static readonly int SpeedZHash = Animator.StringToHash("Speed-Z");
    private static readonly int SpeedXHash = Animator.StringToHash("Speed-X");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");

    private void Awake()
    {
        // Attempt to find the Character component on the GameObject
        this.m_Character = this.GetComponent<Character>();
        if (this.m_Character == null)
        {
            Debug.LogError("GameObjectMovementAnimator requires a Character component.");
            return;
        }

        // Retrieve the Animator from the Character's Animim system
        this.m_Animator = this.m_Character.Animim.Animator;
        if (this.m_Animator == null)
        {
            Debug.LogError("Animator not found in the Character's Animim system.");
        }
    }

    private void Update()
    {
        if (this.m_Character == null || this.m_Animator == null) return;

        // Gather movement data from the Character's Motion system
        Vector3 localMoveDirection = this.m_Character.Driver.LocalMoveDirection;
        bool isGrounded = this.m_Character.Driver.IsGrounded;

        // Calculate movement speed
        float speedZ = localMoveDirection.z;
        float speedX = localMoveDirection.x;

        // Update animator parameters
        this.m_Animator.SetFloat(SpeedZHash, speedZ);
        this.m_Animator.SetFloat(SpeedXHash, speedX);
        this.m_Animator.SetBool(GroundedHash, isGrounded);
    }
}
