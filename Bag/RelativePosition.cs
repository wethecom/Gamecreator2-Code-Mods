using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]
[Title("Relative Position")]
[Description("Determines the relative position of a target GameObject in relation to the player")]

[Category("YourCategory/Relative Position")]
[Parameter("Player", "The GameObject representing the player")]
[Parameter("Target", "The target GameObject to track position relative to the player")]
[Parameter("Location Variable", "Variable to store the relative position")]

[Keywords("Position", "Relative", "Tracking", "Direction")]
[Serializable]
public class InstructionRelativePosition : Instruction
{
    [SerializeField] private PropertyGetGameObject m_Player = GetGameObjectPlayer.Create();
    [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
    [SerializeField] private PropertySetString m_LocationVariable = new PropertySetString();
    [SerializeField] private PropertySetGameObject m_Targeted = new();
    [Header("Startup Instructions")]
    [SerializeField] private InstructionList m_StartupInstructions = new InstructionList();

    public override string Title => $"Determine Relative Position of {m_Target} to {m_Player}";

    protected override async Task Run(Args args)
    {
        // Get the player and target GameObjects using Game Creator properties
        GameObject player = m_Player.Get(args);
        GameObject target = m_Target.Get(args);

        if (player == null || target == null)
        {
            Debug.LogWarning("Player or Target is not set");
            return;
        }

        Vector3 direction = target.transform.position - player.transform.position;
        this.m_Targeted.Set(target,args);
        string relativePosition = "";

        // Check vertical position
        if (direction.y > 0.5f) relativePosition += "High ";
        else if (direction.y < -0.5f) relativePosition += "Low ";
        else relativePosition += "Mid ";

        // Check horizontal position
        if (Vector3.Dot(direction, player.transform.right) > 0.5f) relativePosition += "Right ";
        else if (Vector3.Dot(direction, player.transform.right) < -0.5f) relativePosition += "Left ";
        else relativePosition += "Center ";

        // Check depth position
        if (Vector3.Dot(direction, player.transform.forward) > 0.5f) relativePosition += "Front";
        else if (Vector3.Dot(direction, player.transform.forward) < -0.5f) relativePosition += "Back";

        Debug.Log("The target is " + relativePosition + " the player.");

        // Store the relative position using the specified Game Creator variable
        m_LocationVariable.Set(relativePosition, args);

        // Execute startup instructions if available
       // await this.m_StartupInstructions.Run(args);
    }
}
