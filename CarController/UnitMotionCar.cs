using System;
using UnityEngine;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
[Title("Car Motion")]
[Image(typeof(IconChip), ColorTheme.Type.Blue)]
[Category("Car Motion")]
[Description("Motion system for Car-like movement")]
[Serializable]
public class UnitMotionCar : TUnitMotion
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

