using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    // private int NumOfItemsToSpawn = 33;                      // test variable

    [Header("References")]
    [SerializeField] private GameObject _canvasObj;
    [SerializeField] private GameObject _itemSlotPrefab;        // ItemSlot prefab
    [SerializeField] private TabsManager categoryFiltersTab;
    [SerializeField] private Transform _itemParentTransform;    // Items parent -> In hierarchy: Content
    [SerializeField] private GridLayoutGroup _group;            // Items group
    [SerializeField] private int _extraEmptySlots = 5;          // extral slots
    // [HideInInspector] public List<GameObject> Items = new List<GameObject>();   // Used to manage the items in the current backpack

    // temp
    public List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();


    public bool IsMenuOpen { get; private set; }
    // private bool _itemsHaveSpawned;                             // test variable
    // private bool _slotsHaveSpawned;

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
    }

    private void Start()
    {
        _canvasObj.SetActive(false);

        // initialize card slots
        UpdateInventorySlots();
    }

    private void Update()
    {
        
        // open inventory menu
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }

        if (!IsMenuOpen) return;

        // Improving Keyboard handler logic in selecting items in inventory
        if (EventSystem.current.currentSelectedGameObject == null && LastItemSelected != null)
        {
            if (UserInput.MoveInput.x > 0)
            {
                int add = CalculateXAddition(1);
                HandleNextItemSelection(add);
            }
            else if (UserInput.MoveInput.x < 0)
            {
                int add = CalculateXAddition(-1);
                HandleNextItemSelection(add);
            }
            else if (UserInput.MoveInput.y > 0)
            {
                int add = CalculateYAddition(1);
                HandleNextItemSelection(add);
            }
            else if (UserInput.MoveInput.y < 0)
            {
                int add = CalculateYAddition(-1);
                Debug.Log("MoveInput Y < 0: add = " + add);
                HandleNextItemSelection(add);
            }
        } 

        // handle tab page change
        if(categoryFiltersTab != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                categoryFiltersTab.NavigateTabs(1);
            }
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                categoryFiltersTab.NavigateTabs(-1); 
            }
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

    private void HandleNextItemSelection(int addition)
    {
        int newIndex = LastSelectedIndex + addition;
        if (newIndex < 0)
        {
            EventSystem.current.SetSelectedGameObject(LastItemSelected);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(inventorySlots[newIndex].gameObject);
        }
    }

    private int CalculateXAddition(int direction)
    {
        Vector2Int count = GridLayoutGroupHelper.Size(_group);
        if(direction > 0)
        {
            // if last slot or not
            if (LastSelectedIndex == inventorySlots.Count - 1)
            {
                return 0;
            }
            if (LastSelectedIndex % count.x == count.x - 1)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        if(direction < 0)
        {
            if(LastSelectedIndex % count.x == 0)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        return 0;
    }

    /**        ------------------- 
     *        | 1 | 2 | 3 | 4 | 5 |
     *        | 6 | 7 | 8 | 9 | 10|
     *         -------------------
     *        Suppose the position is at 8. If moving upward, 8 - 5 = 3 > 0, return -5. Adding -5 to the value of LastSelectedIndex: 8 equals 3.
     */
    private int CalculateYAddition(int direction)
    {
        if(direction > 0)
        {
            Vector2Int count = GridLayoutGroupHelper.Size(_group);
            if (LastSelectedIndex - count.x < 0)
            {
                return 0;
            }
            else
            {
                return -count.x;
            }
        }
        else if (direction < 0)
        {
            Debug.Log("direction = " + direction);
            Vector2Int count = GridLayoutGroupHelper.Size(_group);
            // Assure that the last line cannot move downward.
            if (LastSelectedIndex + count.x >= inventorySlots.Count)
            {
                Debug.Log("当前格子大小：" + inventorySlots.Count);
                Debug.Log("要移动的数量：" + count.x);
                Debug.Log("上一个选择的index：" + LastSelectedIndex);
                return 0;
            }
            else
            {
                return count.x;
            }
        }
        return 0;
    }
    
    void ToggleMenu()
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

            //if (!_itemsHaveSpawned)
            //{
            //    // test
            //    SpawnItems();
                
            //}
            //else
            //{
                
            //}
            if (inventorySlots.Count != 0)
                EventSystem.current.SetSelectedGameObject(inventorySlots[0].gameObject);
        }
    }

    public void ChangeFilter(int id)
    {
        if (inventorySlots.Count != 0)
            EventSystem.current.SetSelectedGameObject(inventorySlots[0].gameObject);

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

        // Activate remaining empty slots
        for (int i = items.Count; i < items.Count + _extraEmptySlots; i++)
        {
            if (i < inventorySlots.Count)
            {
                inventorySlots[i].gameObject.SetActive(true);
                inventorySlots[i].Initialize(null); // Initialize as empty
            }
        }

        //for (int i = 0; i < inventorySlots.Count; i++)
        //{
        //    bool isEmpty = (i >= items.Count);
        //    inventorySlots[i].Initialize(isEmpty ? null : items[i]);
        //}
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

    //private void SpawnItems()
    //{
    //    for(int i = 0; i < NumOfItemsToSpawn; i++)
    //    {
    //        GameObject item = Instantiate(_itemsToSpawn, _itemParentTransform);
    //        Items.Add(item);
    //    }

    //    _itemsHaveSpawned = true;
    //}
}
