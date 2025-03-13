using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(0, 0, 1)]
[Title("Store Item Count in Variable")]
[Description("Stores the count of a specific item type in a bag into a variable based on PropertyID")]

[Category("$wethecom/Inventory/Bags/Store Item Count")]
[Parameter("Bag", "The targeted Bag component")]
[Parameter("Item", "The type of item to count")]
[Parameter("Property ID", "The Property ID to filter items")]
[Parameter("Variable", "The variable where the count is stored")]

[Keywords("Bag", "Inventory", "Count", "Store", "Variable")]
[Image(typeof(IconItem), ColorTheme.Type.Green, typeof(OverlayListVariable))]
[Serializable]
public class InstructionInventoryStoreItemCount : Instruction
{
    // MEMBERS: ----
    [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
    [SerializeField] private PropertyGetItem m_Item = new PropertyGetItem();
    [SerializeField] private IdString m_PropertyId = IdString.EMPTY;
    [SerializeField] private PropertySetNumber m_Variable = new PropertySetNumber();

    // PROPERTIES: ----
    public override string Title => $"Store {this.m_Item} Count in {this.m_Variable}";

    // RUN METHOD: ----
    protected override Task Run(Args args)
    {
        // Get the Bag component
        Bag bag = this.m_Bag.Get<Bag>(args);
        if (bag == null) return DefaultResult;

        // Get the Item type
        Item item = this.m_Item.Get(args);
        if (item == null) return DefaultResult;

        // Count items with the specified PropertyID
        int count = bag.Content.CountType(item);
        

        // Store the count in the variable
        this.m_Variable.Set(count, args);

        return DefaultResult;
    }
}