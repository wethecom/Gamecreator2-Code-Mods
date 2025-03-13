using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]
[Title("Aim Gun Using IK")]
[Description("Aims the gun using IK to point towards the center of the camera view")]

[Category("Custom/Combat/Aim Gun Using IK")]

[Parameter("Character", "The character GameObject that will aim the gun")]
[Parameter("Gun Hold Position", "The transform where the gun should be held")]
[Parameter("Aim Speed", "The speed at which the character aims the gun")]

[Keywords("IK", "Aim", "Gun", "Camera", "Third Person")]

//[Image(typeof(IconCrosshair), ColorTheme.Type.Green)]

[Serializable]
public class InstructionAimGunUsingIK : Instruction
{
    [SerializeField]
    private PropertyGetGameObject m_Character = new PropertyGetGameObject();

    [SerializeField]
    private PropertyGetGameObject m_GunHoldPosition = new PropertyGetGameObject();

    [SerializeField]
    private PropertyGetDecimal m_AimSpeed = new PropertyGetDecimal(5f);

    public override string Title => $"Aim gun using IK on {this.m_Character}";

    protected override Task Run(Args args)
    {
        GameObject character = this.m_Character.Get(args);
        GameObject gunHoldPosition = this.m_GunHoldPosition.Get(args);
        float aimSpeed = (float)this.m_AimSpeed.Get(args);

        if (character == null || gunHoldPosition == null) return DefaultResult;

        Animator animator = character.GetComponent<Animator>();
        if (animator == null) return DefaultResult;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 aimPoint = hit.point;
            Vector3 aimDirection = aimPoint - gunHoldPosition.transform.position;
            Quaternion aimRotation = Quaternion.LookRotation(aimDirection);

            // IK Logic (pseudocode)
            // Set IK target position and rotation for aiming the gun
            // This will depend on your specific IK setup and may involve setting IK weights and target positions

            // Example:
             animator.SetIKPosition(AvatarIKGoal.RightHand, aimPoint);
             animator.SetIKRotation(AvatarIKGoal.RightHand, aimRotation);
        }

        return DefaultResult;
    }
}

