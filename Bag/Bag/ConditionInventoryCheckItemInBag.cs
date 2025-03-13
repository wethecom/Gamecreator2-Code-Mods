using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Inventory;
using UnityEngine;
using GameCreator.Runtime.Characters;

namespace GVL.GameCreator.Runtime.Inventory
{
    [Version(1, 0, 2)]

    [Title("Check Item in Bag")]
    [Description("Checks if a specified item exists in the targeted Bag")]

    [Category("Inventory/Check Item in Bag")]

    [Parameter("Item", "The type of item to check for")]
    [Parameter("Bag", "The targeted Bag component")]

    [Keywords("Bag", "Inventory", "Check", "Exists", "Item")]

    [Serializable]
    public class ConditionInventoryCheckItemInBag : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetItem m_Item = new PropertyGetItem();
        [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Check if {this.m_Item} exists in {this.m_Bag}";

        // RUN METHOD: ----------------------------------------------------------------------------
        protected override bool Run(Args args)
        {
            Item item = this.m_Item.Get(args);
            if (item == null) return false;

            Bag bag = this.m_Bag.Get<Bag>(args);
            if (bag == null) return false;

            return bag.Content.ContainsType(item,1);
        }
    }
}