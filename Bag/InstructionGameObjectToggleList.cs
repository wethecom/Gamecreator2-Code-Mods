using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Toggle Game Objects with Main from Local List")]
    [Description("Toggles a list of game objects from a local variable list, with a main object controlling the activation of the rest.")]

    [Category("Game Objects/Toggle Active with Main from Local List")]
    [Keywords("Activate", "Deactivate", "Enable", "Disable", "Toggle", "Main", "Local List")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Yellow)]

    [Serializable]
    public class InstructionGameObjectToggleLocalList : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_MainGameObject = new PropertyGetGameObject();
        [SerializeField] private LocalListVariables localListVariables;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Toggle {this.m_MainGameObject} and Local List of GameObjects";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            GameObject mainGameObject = this.m_MainGameObject.Get(args);

            if (mainGameObject == null || localListVariables == null) return DefaultResult;

            // Get the active state of the main game object
            bool isMainActive = mainGameObject.activeSelf;

            // Toggle the main game object
            mainGameObject.SetActive(!isMainActive);

            // Iterate through the local list of game objects
            for (int i = 0; i < localListVariables.Count; i++)
            {
                GameObject gameObject = localListVariables.Get(i) as GameObject;

                if (gameObject != null && gameObject != mainGameObject)
                {
                    // Toggle the game objects in the list based on the main game object's state
                    gameObject.SetActive(isMainActive);
                }
            }

            return DefaultResult;
        }
    }
}
