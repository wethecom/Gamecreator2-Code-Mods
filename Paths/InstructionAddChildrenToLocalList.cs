using System;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using System.Threading.Tasks;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Add Children to Local List Variables")]
    [Description("Adds all children of the specified parent GameObject to the LocalListVariables.")]
    [Image(typeof(IconFilter), ColorTheme.Type.Teal)]
    [Category("Path/Children to list")]
    [Keywords("Add", "Children", "Local", "List", "Variables")]

    [Serializable]
    public class InstructionAddChildrenToLocalList : Instruction
    {
        [SerializeField]
        //private GameObject m_ParentGameObject; // Reference to the parent GameObject
        private PropertySetGameObject m_ParentGameObject = new PropertySetGameObject(); // Reference to a Variable
        [SerializeField]
        private LocalListVariables m_LocalListVariables; // Reference to the LocalListVariables to store the children

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Add children of {m_ParentGameObject} to {m_LocalListVariables}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            if (m_ParentGameObject == null || m_LocalListVariables == null)
            {
                Debug.LogWarning("Parent GameObject or LocalListVariables is not assigned.");
                return DefaultResult;
            }


            m_LocalListVariables.Clear(); // Clear the existing list first
           
            GameObject path = m_ParentGameObject.Get(args);
            // Iterate through all child objects and add them to the LocalListVariables
            for (int i = 0; i < path.transform.childCount; i++)
            {
                Transform child = path.transform.GetChild(i);
               // m_LocalListVariables.Set(i, child.gameObject);
                m_LocalListVariables.Insert(i, child.gameObject);
            }

            return DefaultResult;
        }
    }
}
