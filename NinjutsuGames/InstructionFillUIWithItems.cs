using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using TMPro;


[Version(1, 0, 2)]
    [Title("Fill UI with Item")]
    [Description("Fills a UI with Items from a specified Bag and you can Equip/Use them on button click")]
    [Category("$wethecom/Inventory/Fill UI with Items")]
    [Keywords("Weapons", "Fill", "UI", "Inventory", "Items", "Bag")]
    [Image(typeof(IconInstructions), ColorTheme.Type.Blue, typeof(OverlayListVariable))]

    [Serializable]
    public class InstructionFillUIWithItems : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Bag = new PropertyGetGameObject();
    [SerializeField] private ItemAction m_ItemAct = new ItemAction();    
    [SerializeField] public PropertyGetBool m_Use = new PropertyGetBool();
        [SerializeField] public PropertyGetBool m_Equip = new PropertyGetBool();
        [SerializeField] public PropertyGetString m_Property = new PropertyGetString();
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private GameObject panelPrefab;
        [SerializeField] private GameObject canvasPrefab ;
        [SerializeField] private Vector2 Pos;
    private Canvas canvasInstance;
        private RectTransform panelInstance;
        private string property;
       
   
    public enum ItemAction
    {
        Use,
        Equip,
        Drop
    }


    protected override Task Run(Args args)
        {
           
            property = this.m_Property.Get(args);   
            if (!ValidateComponents()) return DefaultResult;

            Bag bag = this.m_Bag.Get<Bag>(args);
            if (bag == null)
            {
                Debug.LogError("Bag component not found.");
                return DefaultResult;
            }

            CreateUIHierarchy(args);
            CreateItemButtons(bag, args);

            return DefaultResult;
        }

        private bool ValidateComponents()
        {
            if (buttonPrefab == null || panelPrefab == null || canvasPrefab == null)
            {
                Debug.LogError("Button, panel, or canvas prefab is not assigned");
                return false;
            }
            return true;
        }

        private void CreateUIHierarchy(Args args)
        {
            // Clean up existing instances
            if (canvasInstance != null) GameObject.Destroy(canvasInstance.gameObject);
            
            // Create canvas
            GameObject canvasGO = GameObject.Instantiate(canvasPrefab, args.Self.transform);
        
        canvasInstance = canvasGO.GetComponent<Canvas>();

            // Setup canvas
            canvasInstance.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Position canvas to top-left
            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.zero;
            canvasRect.pivot = Vector2.zero;
            canvasRect.anchoredPosition = Vector2.zero;

            // Create panel
            GameObject panelGO = GameObject.Instantiate(panelPrefab, canvasInstance.transform);
        // Ensure it's a UI element inside a Canvas
        RectTransform rectTransform = panelGO.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += Pos;
        }
        
        panelInstance = panelGO.GetComponent<RectTransform>();

            // Position panel relative to top-left
           // panelInstance.anchorMin = Vector2.zero;
           // panelInstance.anchorMax = Vector2.zero;
           // panelInstance.pivot = Vector2.zero;
           // panelInstance.anchoredPosition = new Vector2(0, -10); // Small offset from top-left corner
        }

        private void CreateItemButtons(Bag bag, Args args)
        {
            // Clear existing buttons
            foreach (Transform child in panelInstance)
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (Cell cell in bag.Content.CellList)
            {
                if (cell == null || cell.Available) continue;

                RuntimeItem rootItem = cell.RootRuntimeItem;
                if (rootItem != null && HasPropertyID(rootItem.Item))
                {
                    CreateButtonForItem(rootItem, bag, args);
                }
            }
        }

        private bool HasPropertyID(Item item)
        {
            var properties = item.Properties;
            for (int i = 0; i < properties.ListLength; i++)
            {
                if (properties.Get(i).ID.ToString().Equals(property, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void CreateButtonForItem(RuntimeItem runtimeItem, Bag bag, Args args)
        {
            GameObject buttonGO = GameObject.Instantiate(buttonPrefab, panelInstance);

            // Setup button image
            Image buttonImage = buttonGO.GetComponentInChildren<Image>();
     

        if (buttonImage != null)
            {
                buttonImage.sprite = runtimeItem.Item.Info.Sprite(args);
            }

            // Setup button
            Button button = buttonGO.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ItemUsage(runtimeItem, bag, args));
        }
        else
        {
            Debug.LogError("Button component not found in prefab");
        }
        TextMeshProUGUI Stack = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
        Stack.SetText(bag.Content.CountType(runtimeItem.Item).ToString());//
    }

    private async void ItemUsage(RuntimeItem runtimeItem, Bag bag, Args args)
        {

        switch (m_ItemAct)
        {
            case ItemAction.Use:
                bag.Content.Use(runtimeItem);
                canvasInstance.gameObject.SetActive(false);
                // await Usage.RunOnUse(runtimeItem.Item, args);
                Debug.Log($"Used Item: {runtimeItem.Item.name}");
                Console.WriteLine($"Using {runtimeItem.ItemID}.");
                break;


            case ItemAction.Equip:

                if (bag.Equipment.CanEquip(runtimeItem) )
                {
                    await bag.Equipment.Equip(runtimeItem);
                    Debug.Log($"Equipped item: {runtimeItem.Item.name}");
                }
                else
                {
                    Debug.LogWarning($"Cannot equip item: {runtimeItem.Item.name}");
                }

                Console.WriteLine($"Equipping {runtimeItem.ItemID}.");
                break;
            case ItemAction.Drop:
                
                RuntimeItem removedItem = bag.Content.RemoveType(runtimeItem.Item);
                Console.WriteLine($"Dropping {runtimeItem.ItemID}.");
                break;
            default:
                Console.WriteLine("Unknown action.");
                break;
        }



        
           
        }
    public int GetStackCount(Bag bag, RuntimeItem runtimeItem)
    {
        if (bag == null || runtimeItem == null)
        {
            Debug.LogWarning("Bag or RuntimeItem is null");
            return 0;
        }

        // Find the position of the RuntimeItem in the bag
        Vector2Int position = bag.Content.FindPosition(runtimeItem.RuntimeID);
        if (position == Vector2Int.zero)
        {
            Debug.LogWarning("RuntimeItem not found in the bag");
            return 0;
        }
       
        // Get the Cell at the position
        Cell cell = bag.Content.GetContent(position);
        if (cell == null || cell.Available)
        {
            Debug.LogWarning("Cell is empty or unavailable");
            return 0;
        }

        // Return the stack count for the specific RuntimeItem
        return cell.Count;
    }
}