using UnityEngine;
using UnityEngine.UI;

public class HotbarSelector : MonoBehaviour
{
    [SerializeField] private GameObject[] hotbarSlots; // Fixed hotbar slots UI elements (5 slots)
    [SerializeField] private HotBars[] hotbarElements = new HotBars[5]; // Array to store weapons and UI frames

    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;

    private int currentIndex = 0; // Currently selected index
    private GameObject currentSelected; // Currently selected weapon

    void Start()
    {
        UpdateHotbarSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) SelectPrevious();
        if (Input.GetKeyDown(KeyCode.D)) SelectNext();
    }

    // Add weapon to the hotbar at the next available slot, replacing the oldest if needed
    public void AddWeaponToHotbar(GameObject weapon, Sprite weaponIcon)
    {
        // Shift elements to the left to make room for the new weapon
        for (int i = 0; i < hotbarElements.Length - 1; i++)
        {
            hotbarElements[i] = hotbarElements[i + 1];
        }

        // Add the new weapon to the last slot
        hotbarElements[hotbarElements.Length - 1] = new HotBars
        {
            weapon = weapon,
            parent = hotbarSlots[hotbarElements.Length - 1],  // Reference to the UI element
            frame = hotbarSlots[hotbarElements.Length - 1].transform.Find("Frame").gameObject,
            icon = hotbarSlots[hotbarElements.Length - 1].transform.Find("Icon").GetComponent<Image>() // Reference to the icon image
        };

        // Update the icon of the new weapon
        hotbarElements[hotbarElements.Length - 1].icon.sprite = weaponIcon;

        UpdateHotbarSelection();
    }

    // Remove weapon from the hotbar at a specific index
    public void RemoveWeaponFromHotbar(int index)
    {
        if (index < 0 || index >= hotbarElements.Length)
        {
            Debug.LogWarning("Invalid hotbar index!");
            return;
        }

        // Reset the icon and weapon
        hotbarElements[index].icon.sprite = null;
        hotbarElements[index].weapon = null;

        // Shift remaining elements to fill the slot
        for (int i = index; i < hotbarElements.Length - 1; i++)
        {
            hotbarElements[i] = hotbarElements[i + 1];
        }

        hotbarElements[hotbarElements.Length - 1] = null;

        UpdateHotbarSelection();
    }

    // Switch to the previous weapon in the hotbar
    private void SelectPrevious()
    {
        currentIndex = (currentIndex - 1 + hotbarElements.Length) % hotbarElements.Length;
        UpdateHotbarSelection();
    }

    // Switch to the next weapon in the hotbar
    private void SelectNext()
    {
        currentIndex = (currentIndex + 1) % hotbarElements.Length;
        UpdateHotbarSelection();
    }

    // Update the UI and selected weapon based on the current index
    private void UpdateHotbarSelection()
    {
        // Get the current selected weapon
        currentSelected = (currentIndex < hotbarElements.Length && hotbarElements[currentIndex] != null) ? hotbarElements[currentIndex].weapon : null;

        // Update UI frame colors based on selection
        for (int i = 0; i < hotbarElements.Length; i++)
        {
            if (hotbarElements[i] != null && hotbarElements[i].frame != null)
            {
                Image image = hotbarElements[i].frame.GetComponent<Image>();
                if (image != null)
                {
                    image.color = (i == currentIndex) ? selectedColor : defaultColor;
                }
            }
        }
    }

    // Return the currently selected weapon
    public GameObject GetCurrentSelected()
    {
        return currentSelected;
    }
}
