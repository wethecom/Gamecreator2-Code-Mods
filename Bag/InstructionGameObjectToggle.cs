using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Toggle Game 2 Objects")]
    [Description("Toggles the active state of two game objects. Activates one and deactivates the other.")]

    [Category("Game Objects/Toggle Active Game Obgects")]
    [Keywords("Activate", "Deactivate", "Enable", "Disable", "Toggle")]

    [Image(typeof(IconCubeSolid), ColorTheme.Type.Yellow)]

    [Serializable]
    public class InstructionGameObjectToggle : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_GameObjectA = new PropertyGetGameObject();
        [SerializeField] private PropertyGetGameObject m_GameObjectB = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Toggle {this.m_GameObjectA} and {this.m_GameObjectB}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            GameObject gameObjectA = this.m_GameObjectA.Get(args);
            GameObject gameObjectB = this.m_GameObjectB.Get(args);

            if (gameObjectA == null || gameObjectB == null) return DefaultResult;

            bool isActiveA = gameObjectA.activeSelf;

            // Toggle the active states
            gameObjectA.SetActive(!isActiveA);
            gameObjectB.SetActive(isActiveA);

            return DefaultResult;
        }
    }
}

