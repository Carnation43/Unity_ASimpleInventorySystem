using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [Header("Inventory UI")]
    [SerializeField] private GameObject _canvasObj;
    [SerializeField] private GameObject _itemSlotPrefab;        // ItemSlot prefab
    [SerializeField] private TabsManager categoryFiltersTab;
    [SerializeField] private Transform _itemParentTransform;    // Items parent -> In hierarchy: Content
    [SerializeField] public GridLayoutGroup _group;             // Items group
    [SerializeField] private int _extraEmptySlots = 5;          // extral slots


    [Header("Controllers")]
    [SerializeField] private InventoryNavigationHandler _navigationHandler; // Improve backpack slots navigation

    // temp
    public List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();


    public bool IsMenuOpen { get; private set; }

    // record selecting item
    public GameObject LastItemSelected { get; set; }
    public int LastSelectedIndex { get; set; }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        inventorySlots = new List<InventorySlotUI>(_group.GetComponentsInChildren<InventorySlotUI>());

        InventoryManager.instance.OnItemAdded += UpdateInventorySlots;
        InventoryManager.instance.OnItemRemoved += UpdateInventorySlots;

        _navigationHandler.Initialize(this);
    }

    private void Start()
    {
        _canvasObj.SetActive(false);
        IsMenuOpen = false;
        // initialize card slots
        // UpdateInventorySlots();
    }

    private void Update()
    {
        
        // open inventory menu
        //if (Keyboard.current.mKey.wasPressedThisFrame)
        //{
        //    ToggleMenu();
        //}

        if (!IsMenuOpen) return;

        // handle selecting items logic
        if (EventSystem.current.currentSelectedGameObject == null && LastItemSelected != null)
        {
            _navigationHandler.HandleNavigationInput();
        } 
    }

    // dynamically update inventory slots
    public void UpdateInventorySlots()
    {
        if (_itemSlotPrefab == null || _itemParentTransform == null)
        {
            Debug.LogError("References missing!");
            return;
        }

        if (!IsMenuOpen) return;

        var items = InventoryManager.instance.inventory;
        int neededSlots = items.Count + _extraEmptySlots;

        // get current numbers of slot
        //inventorySlots = _itemParentTransform.GetComponentsInChildren<InventorySlotUI>();
        //int currentSlotCount = inventorySlots.Length;

        // generate new slots (counts: _extraEmptySlots)
        while (inventorySlots.Count < neededSlots)
        {
            GameObject newSlotObj = Instantiate(_itemSlotPrefab, _itemParentTransform);
            inventorySlots.Add(newSlotObj.GetComponent<InventorySlotUI>());
        }

        // Remove excess slots if inventory shrinks
        while (inventorySlots.Count > neededSlots)
        {
            int lastIndex = inventorySlots.Count - 1;
            Destroy(inventorySlots[lastIndex].gameObject);
            inventorySlots.RemoveAt(lastIndex);
        }

        // Classify according to the item category and Initialize all slots
        ChangeFilter(0);

        // reset filter option
        categoryFiltersTab.SelectTab(0);
    }
    
    public void ToggleMenu()
    {
        if (IsMenuOpen)
        {
            if (LastItemSelected != null)
            {
                BaseIconAnimationController iconController = LastItemSelected.GetComponent<BaseIconAnimationController>();
                if (iconController != null)
                {
                    // Ensure that kill animation before selecting items.
                    iconController.OnDeselect(new BaseEventData(EventSystem.current));
                }
            }
            EventSystem.current.SetSelectedGameObject(null);

            _canvasObj?.SetActive(false);
            IsMenuOpen = false;      
        }
        else
        {
            _canvasObj.SetActive(true);
            IsMenuOpen = true;


            // update slot when open the menu
            UpdateInventorySlots();
        }
    }

    public void ChangeFilter(int id)
    {
        // TooltipInstance.instance.Hide();

        // 1. Clear selection and hide tooltip at the beginning
        EventSystem.current.SetSelectedGameObject(null);
        // TooltipInstance.instance.Hide();

        //if (inventorySlots.Count > 0)
        //{
        //    Debug.Log("InventorySlots.Count = " + inventorySlots.Count);
        //    EventSystem.current.SetSelectedGameObject(inventorySlots[0].gameObject);
        //}

        var items = InventoryManager.instance.inventory;

        Debug.Log($"ChangeFilter received id: {id}");

        if (id != 0)
        {
            ItemCategory category = (ItemCategory)(id - 1);
            items = InventoryManager.instance.inventory.FindAll(x => x.item.category == category);
        }

        foreach (var slot in inventorySlots)
        {
            slot.gameObject.SetActive(false);
        }

        // Activate and initialize only the necessary slots
        for (int i = 0; i < items.Count; i++)
        {
            if (i < inventorySlots.Count)
            {
                inventorySlots[i].gameObject.SetActive(true);
                inventorySlots[i].Initialize(items[i]);
            }
        }

        // Activate remaining empty slots (no longer need now, but still keep)
        for (int i = items.Count; i < items.Count + _extraEmptySlots; i++)
        {
            if (i < inventorySlots.Count)
            {
                inventorySlots[i].gameObject.SetActive(true);
                inventorySlots[i].Initialize(null); // Initialize as empty
            }
        }

        // 2. Start a coroutine to select the first item after a delay
        StartCoroutine(SelectFirstItemAfterDelay());
    }

    private IEnumerator SelectFirstItemAfterDelay()
    {
        yield return null; // Wait for one frame

        if (inventorySlots.Count > 0)
        {
            Debug.Log("inventorySlots Count: " + inventorySlots.Count);
            // Find the first active and interactable slot
            InventorySlotUI firstActiveSlot = null;
            foreach (var slot in inventorySlots)
            {
                if (slot.gameObject.activeInHierarchy && slot.GetComponent<Selectable>() != null && slot.GetComponent<Selectable>().interactable)
                {
                    firstActiveSlot = slot;
                    break;
                }
            }

            if (firstActiveSlot != null)
            {
                EventSystem.current.SetSelectedGameObject(firstActiveSlot.gameObject);
            }
            else
            {
                Debug.Log("No objects found!");
                EventSystem.current.SetSelectedGameObject(null);
                TooltipInstance.instance.Hide();
            }
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            TooltipInstance.instance.Hide();
        }
    }

    private void OnDestroy()
    {
        // remove events
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.OnItemAdded -= UpdateInventorySlots;
            InventoryManager.instance.OnItemRemoved -= UpdateInventorySlots;
        }
    }
}
