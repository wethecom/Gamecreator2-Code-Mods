using System;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Inventory.UnityUI;
using UnityEngine;

public class BagSlotDisplay : MonoBehaviour
{
    [SerializeField] private Bag bag; // Reference to the Bag component
    [SerializeField] private BagCellUI bagCellUI; // Reference to the BagCellUI to display the item
    [SerializeField] private int slotIndex; // The slot index to retrieve the item from

    private void Start()
    {
        // Display the item in the specified slot index
        DisplayItemInSlot(slotIndex);
    }

    public void DisplayItemInSlot(int index)
    {
        if (bag == null || bagCellUI == null)
        {
            Debug.LogWarning("Bag or BagCellUI is not assigned.");
            return;
        }

        // Get the item from the bag's content using the slot index
        Vector2Int position = new Vector2Int(index % bag.Shape.MaxWidth, index / bag.Shape.MaxWidth);
        Cell cell = bag.Content.GetContent(position);

        if (cell == null || cell.Available)
        {
            Debug.Log($"No item found in slot index {index}.");
            bagCellUI.RefreshUI(1,1); // Clear the BagCellUI
            return;
        }

        // Get the runtime item from the cell
        RuntimeItem runtimeItem = cell.Peek();

        if (runtimeItem != null)
        {
            // Update the BagCellUI with the item's data
            bagCellUI.RefreshUI(1,1);
            Debug.Log($"Displayed item '{runtimeItem.Item.Info.Name(bag.Args)}' in slot index {index}.");
        }
        else
        {
            Debug.Log($"No runtime item found in slot index {index}.");
            bagCellUI.RefreshUI(1,1); // Clear the BagCellUI
        }
    }
}