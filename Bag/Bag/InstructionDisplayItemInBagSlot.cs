using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Inventory.UnityUI;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(0, 0, 1)]
[Title("Display Item in Bag Slot")]
[Description("Displays an item from a specific slot index in a Bag into a BagCellUI")]

[Category("Inventory/Bags/Display Item in Slot")]

[Parameter("Bag", "The Bag component to retrieve the item from")]
[Parameter("Bag Cell UI", "The BagCellUI to display the item")]
[Parameter("Slot Index", "The slot index to retrieve the item from")]

[Keywords("Bag", "Inventory", "Slot", "Display", "Item")]
[Image(typeof(IconBagOutline), ColorTheme.Type.Green)]
[Serializable]
public class InstructionDisplayItemInBagSlot : Instruction
{
    // PARAMETERS: ----
    [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
    [SerializeField] private PropertyGetGameObject m_BagCellUI = GetGameObjectNone.Create();
    [SerializeField] private PropertyGetInteger m_SlotIndex = new PropertyGetInteger(0);

    public override string Title => $"Display Item in Slot {this.m_SlotIndex}";

    // RUN METHOD: ----
    protected override Task Run(Args args)
    {
        // Get the Bag and BagCellUI components
        Bag bag = this.m_Bag.Get<Bag>(args);
        BagCellUI bagCellUI = this.m_BagCellUI.Get<BagCellUI>(args);
        int slotIndex = 0;//(int)this.m_SlotIndex.Get(args);

        if (bag == null || bagCellUI == null)
        {
            Debug.LogWarning("Bag or BagCellUI is not assigned.");
            return DefaultResult;
        }

        // Calculate the position in the grid based on the slot index
        Vector2Int position = new Vector2Int(slotIndex % bag.Shape.MaxWidth, slotIndex / bag.Shape.MaxWidth);
        Cell cell = bag.Content.GetContent(position);

        if (cell == null || cell.Available)
        {
            Debug.Log($"No item found in slot index {slotIndex}.");
            bagCellUI.RefreshUI(1, 1); // Clear the BagCellUI
            return DefaultResult;
        }

        // Get the runtime item from the cell
        RuntimeItem runtimeItem = cell.Peek();

        if (runtimeItem != null)
        {
            // Update the BagCellUI with the item's data
            bagCellUI.RefreshUI(position.x, position.y);
            Debug.Log($"Displayed item '{runtimeItem.Item.Info.Name(bag.Args)}' in slot index {slotIndex}.");
        }
        else
        {
            Debug.Log($"No runtime item found in slot index {slotIndex}.");
            bagCellUI.RefreshUI(1, 1); // Clear the BagCellUI
        }

        return DefaultResult;
    }
}