using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Inventory.UnityUI;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]
[Title("Retrieve and Set Item")]

[Description("Retrieves an item from a BagCellUI and sets it to a Item variable")]

[Category("Inventory/Retrieve and Set Item")]

[Keywords("Item", "Cell", "Variable", "Get", "Retrieve")]
[Image(typeof(IconItem), ColorTheme.Type.Green)]

[Serializable]
public class InstructionRetrieveItemFromBagUICell : Instruction
{
    // MEMBERS: -------------------------------------------------------------------------------
    [SerializeField] private BagCellUI m_BagCellUI;
    [SerializeField] private PropertySetItem m_SetItem;
    
    // PROPERTIES: ----------------------------------------------------------------------------
    
    public override string Title => $"Retrieve {this.m_BagCellUI}";
    
    // RUN METHOD: ----------------------------------------------------------------------------
    protected override Task Run(Args args)
    {
        // Access the Cell associated with the BagCellUI
        var cell = m_BagCellUI.Cell;

        // Retrieve the Item stored in the Cell
        Item item = cell?.Item;

        // Check if an item exists
        if (item is not null)
        {
            // Set the retrieved item to the local variable
            this.m_SetItem.Set(item, args);
        }

        return Task.CompletedTask;
    }
}