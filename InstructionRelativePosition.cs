using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Relative Position")]
    [Description("Calculates the relative position of a target game object to a source game object and stores the result as a string")]

    [Category("$wethecom/Relative Position")]
    [Parameter("Source", "The source game object used as the reference point")]
    [Parameter("Target", "The target game object whose position is evaluated")]
    [Parameter("Set", "The string variable where the result is stored")]

    [Keywords("Direction", "Relative", "Position", "String", "Concatenate")]
    [Image(typeof(IconString), ColorTheme.Type.Yellow, typeof(OverlayArrowRight))]

    [Serializable]
    public class InstructionRelativePosition : Instruction
    {
        // MEMBERS: ----
        [SerializeField] private PropertyGetGameObject m_Source = GetGameObjectSelf.Create();
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectInstance.Create();
        [SerializeField] private PropertySetString m_Set = new PropertySetString();

        // PROPERTIES: ----
        public override string Title => $"Set {this.m_Set} = Relative Position of {this.m_Target} to {this.m_Source}";

        // RUN METHOD: ----
        protected override Task Run(Args args)
        {
            GameObject source = this.m_Source.Get(args);
            GameObject target = this.m_Target.Get(args);

            if (source == null || target == null)
            {
                Debug.LogWarning("Source or Target game object is null");
                return DefaultResult;
            }

            // Calculate the direction vector from source to target
            Vector3 direction = target.transform.position - source.transform.position;

            // Project the direction vector onto the source's local axes
            Vector3 localDirection = source.transform.InverseTransformDirection(direction);

            // Determine the grid coordinates based on the local direction
            int x = Mathf.RoundToInt(Mathf.Clamp(localDirection.x, -1, 1));
            int y = Mathf.RoundToInt(Mathf.Clamp(localDirection.y, -1, 1));
            int z = Mathf.RoundToInt(Mathf.Clamp(localDirection.z, -1, 1));

            // Determine the direction based on the coordinates
            string horizontal = x == -1 ? "Left" : x == 1 ? "Right" : "Center";
            string vertical = y == -1 ? "Below" : y == 1 ? "Above" : "Middle";
            string depth = z == -1 ? "Back" : z == 1 ? "Front" : "Center";

            // Combine the directions
            string result = $"{vertical} {horizontal} {depth}";

            // Store the result in the string variable
            this.m_Set.Set(result, args);

            return DefaultResult;
        }
    }
}
