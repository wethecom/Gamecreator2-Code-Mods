using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(0,0,1)]

[Title("Is Equipped at Index")]
[Description(
    "Returns true if the Bag's wearer has a Runtime Item equipped at the specified equipment " +
    "Index"
)]

[Category("Inventory/Equipment/Is Equipped at Index")]
    
[Parameter("Runtime Item", "The runtime item type to check")]
[Parameter("Bag", "The targeted Bag")]
[Parameter("Equip Index", "The integer number representing the equipment index to check")]

[Keywords("Inventory", "Wears")]
    
[Image(typeof(IconEquipment), ColorTheme.Type.Blue)]
[Serializable]
public class ConditionInventoryIsEquippedAt : Condition
{
    // EXPOSED MEMBERS: ---------------------------------------------------------------------------

    [SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = new PropertyGetRuntimeItem();
    
    [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
    [SerializeField] private PropertyGetInteger m_EquipIndex = GetDecimalInteger.Create(0); 

    // PROPERTIES: --------------------------------------------------------------------------------
        
    protected override string Summary => $"is {this.m_RuntimeItem} on {this.m_Bag}[{this.m_EquipIndex}]";
        
    // RUN METHOD: --------------------------------------------------------------------------------

    protected override bool Run(Args args)
    {
        RuntimeItem runtimeItem = this.m_RuntimeItem.Get(args);
        if (runtimeItem == null) return false;

        Bag bag = this.m_Bag.Get<Bag>(args);
        if (bag == null) return false;
        
        int index = bag.Equipment.GetEquippedIndex(runtimeItem);
        return index == (int) this.m_EquipIndex.Get(args);
    }
}