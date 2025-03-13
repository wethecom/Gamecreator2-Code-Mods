using System;
using UnityEngine;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;

[Title("Spaceship Facing")]
[Image(typeof(IconGamepadCross), ColorTheme.Type.Yellow)]
[Category("Spaceship Facing")]
[Description("Facing system for spaceship-like rotation")]

[Serializable]
public class UnitFacingSpaceship : TUnitFacing
{
    [SerializeField] private float m_RotationSpeed = 90f;
    [SerializeField] private Axonometry m_Axonometry = new Axonometry();

    public override Axonometry Axonometry
    {
        get => m_Axonometry;
        set => m_Axonometry = value;
    }

    protected override Vector3 GetDefaultDirection()
    {
        // Use input direction for facing
        Vector3 inputDirection = this.Character.Player.InputDirection;
        return inputDirection.sqrMagnitude > 0f ? inputDirection.normalized : this.Transform.forward;
    }

    public override void OnUpdate()
    {
        Vector3 targetDirection = this.GetDefaultDirection();
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        this.Transform.rotation = Quaternion.RotateTowards(
            this.Transform.rotation,
            targetRotation,
            m_RotationSpeed * this.Character.Time.DeltaTime
        );
    }
}