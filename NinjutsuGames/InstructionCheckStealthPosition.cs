using System;
using System.Threading.Tasks;
using System.Text;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Title("Check Stealth Position")]
    [Description("Checks and stores all possible stealth attack positions relative to a target")]

    [Category("$wethecom/Stealth Position")]

    [Parameter("Player", "The player character to check position from")]
    [Parameter("Target", "The target GameObject to check stealth attack position against")]
    [Parameter("Height Threshold", "The minimum height difference to consider above/below positions")]
    [Parameter("Angle Threshold", "The maximum angle to determine front/behind/side positions")]
    [Parameter("Store Result", "The variable to store the position result in")]

    [Keywords("Stealth", "Position", "Attack", "Behind", "Above", "Below", "Left", "Right")]

    [Serializable]
    public class InstructionCheckStealthPosition : Instruction
    {
        // MEMBERS: ----
        [SerializeField] private PropertyGetGameObject m_Player = GetGameObjectTarget.Create();
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
        [SerializeField] private float m_HeightThreshold = 1.5f;
        [SerializeField] private float m_AngleThreshold = 60f;
        [SerializeField] private PropertySetString m_RelativePosition = new PropertySetString();

        protected override Task Run(Args args)
        {
            var target = m_Target.Get(args);
            var player = m_Player.Get(args);

            if (target == null || player == null)
            {
                m_RelativePosition.Set("None", args);
                return DefaultResult;
            }

            var positions = new StringBuilder();

            // Check height position
            float heightDiff = player.transform.position.y - target.transform.position.y;
            if (heightDiff > m_HeightThreshold)
            {
                positions.Append("Above");
            }
            else if (heightDiff < -m_HeightThreshold)
            {
                positions.Append("Below");
            }

            // Get direction to player relative to target's forward
            Vector3 targetForward = target.transform.forward;
            Vector3 targetRight = target.transform.right;
            Vector3 toPlayer = (player.transform.position - target.transform.position).normalized;

            // Check horizontal angle
            float forwardAngle = Vector3.Angle(targetForward, toPlayer);
            float rightAngle = Vector3.Angle(targetRight, toPlayer);

            // Add position separator if needed
            if (positions.Length > 0) positions.Append("_");

            // Determine horizontal position
            if (forwardAngle < m_AngleThreshold)
            {
                positions.Append("Front");
            }
            else if (forwardAngle > 180 - m_AngleThreshold)
            {
                positions.Append("Behind");
            }

            // Add left/right position
            if (positions.Length > 0) positions.Append("_");

            // Determine left/right using right angle and cross product
            Vector3 cross = Vector3.Cross(targetForward, toPlayer);
            if (cross.y > 0)
            {
                positions.Append("Left");
            }
            else
            {
                positions.Append("Right");
            }

            // Store the result
            string finalPosition = positions.Length > 0 ? positions.ToString() : "None";
            m_RelativePosition.Set(finalPosition, args);

            return DefaultResult;
        }
    }
}

/*
 Vertical_Horizontal_Lateral combinations:

1. Above_Front_Left
2. Above_Front_Right
3. Above_Behind_Left
4. Above_Behind_Right
5. Below_Front_Left
6. Below_Front_Right
7. Below_Behind_Left
8. Below_Behind_Right
Vertical_Lateral combinations:
9. Above_Left
10. Above_Right
11. Below_Left
12. Below_Right

Horizontal_Lateral combinations:
13. Front_Left
14. Front_Right
15. Behind_Left
16. Behind_Right

Single positions:
17. Above
18. Below
19. Front
20. Behind
21. Left
22. Right
23. None

The instruction uses PropertySetString to store the result 36 and follows proper GameCreator2 instruction patterns
 
 */