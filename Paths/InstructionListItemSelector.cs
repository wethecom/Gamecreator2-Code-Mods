using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("List Item Selector")]
    [Description("Selects an item from a list based on a specified method (first, last, by index, random, closest, or farthest) and assigns it to a named variable.")]
    [Image(typeof(IconFilter), ColorTheme.Type.Teal)]
    [Category("Path/List Paths and Select")]
    [Keywords("Select", "Extract", "List", "Enumeration", "Item", "Variables")]

    [Serializable]
    public class InstructionListItemSelector : Instruction
    {
        [SerializeField]
        private LocalListVariables localListVariables; // List of GameObjects
        [SerializeField]
        private PropertySetGameObject m_NameVariables = new PropertySetGameObject(); // Reference to a Variable
        [SerializeField]
        private Transform m_ReferencePoint; // Reference point for closest/farthest selection

        // Enum to define the method of selection from the list
        public enum SelectionMethod
        {
            FirstItem,
            LastItem,
            ByIndex,
            RandomItem,
            Closest,  // Select closest item to reference point
            Farthest  // Select farthest item from reference point
        }

        [SerializeField]
        private SelectionMethod m_SelectionMethod; // Method of selecting the item

        [SerializeField]
        private int m_ItemIndex; // Index of the item, used if SelectionMethod is ByIndex

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Select item from Local List using {m_SelectionMethod} and assign to {m_NameVariables}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            // Ensure the local list variables are not null or empty
            if (localListVariables == null || localListVariables.Count == 0) return DefaultResult;

            GameObject selectedItem = null;

            switch (m_SelectionMethod)
            {
                case SelectionMethod.FirstItem:
                    selectedItem = localListVariables.Get(0) as GameObject;
                    break;
                case SelectionMethod.LastItem:
                    selectedItem = localListVariables.Get(localListVariables.Count - 1) as GameObject;
                    break;
                case SelectionMethod.ByIndex:
                    if (m_ItemIndex >= 0 && m_ItemIndex < localListVariables.Count)
                    {
                        selectedItem = localListVariables.Get(m_ItemIndex) as GameObject;
                    }
                    break;
                case SelectionMethod.RandomItem:
                    selectedItem = localListVariables.Get(UnityEngine.Random.Range(0, localListVariables.Count)) as GameObject;
                    break;
                case SelectionMethod.Closest:
                    selectedItem = GetClosestOrFarthestItem(isClosest: true);
                    break;
                case SelectionMethod.Farthest:
                    selectedItem = GetClosestOrFarthestItem(isClosest: false);
                    break;
            }

            if (selectedItem != null)
            {
                // Set the named variable with the selected item
                m_NameVariables.Set(selectedItem, args);
            }

            return DefaultResult;
        }

        private GameObject GetClosestOrFarthestItem(bool isClosest)
        {
            GameObject result = null;
            float bestDistance = isClosest ? float.MaxValue : float.MinValue;

            for (int i = 0; i < localListVariables.Count; i++)
            {
                GameObject item = localListVariables.Get(i) as GameObject;
                if (item == null) continue;

                float distance = Vector3.Distance(m_ReferencePoint.position, item.transform.position);
                if (isClosest && distance < bestDistance)
                {
                    bestDistance = distance;
                    result = item;
                }
                else if (!isClosest && distance > bestDistance)
                {
                    bestDistance = distance;
                    result = item;
                }
            }

            return result;
        }
    }
}
