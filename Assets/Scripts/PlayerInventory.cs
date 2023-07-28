using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{

    [SerializeField] private List<Item> allItems;
    [SerializeField] private Image tileInHandObj;
    [SerializeField] private Color colorIfStackEmpty;

    private int currentInventorySlot;

    public List<ItemStack> Inventory;

    private void Awake()
    {
        GameUtilities.AllItems = new Dictionary<string, Item>();
        foreach (Item item in allItems)
        {
            GameUtilities.AllItems.Add(item.ItemID, item);
        }
    }

    private void Start()
    {
        Inventory = new List<ItemStack>()
        {
            new ItemStack(GameUtilities.AllItems["Pickaxe"], 1)
        };

        UpdateInventoryUI();
    }

    public ItemStack GetCurrentStack()
    {
        return Inventory[currentInventorySlot];
    }

    public void AddItems(Item newItem, int amount)
    {
        ItemStack _stack = null;

        foreach (ItemStack _curStack in Inventory)
        {
            if (_curStack.Item.ItemID == newItem.ItemID)
            {
                _stack = _curStack;
                break;
            }
        }

        if (_stack == null)
        {
            Inventory.Add(new ItemStack(newItem, amount));
            return;
        }

        _stack.Count += amount;
        UpdateInventoryUI();
    }

    public void RemoveItems(Item newItem, int amount)
    {
        ItemStack _stack = null;

        int itemIndex = 0;
        foreach (ItemStack _curStack in Inventory)
        {
            if (_curStack.Item.ItemID == newItem.ItemID)
            {
                _stack = _curStack;
                break;
            }

            itemIndex++;
        }

        if (_stack != null)
        {
            _stack.Count -= amount;

            if (_stack.Count <= 0)
            {
                ItemType _previousStackType = Inventory[currentInventorySlot].Item.Type;

                Inventory.RemoveAt(itemIndex);
                if (currentInventorySlot >= itemIndex) CycleSlots(-1);

                CycleToNextItemOfType(_previousStackType);
            }

            UpdateInventoryUI();
        }
    }

    public void CycleHotbar(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector2 _mousePos = Input.mousePosition;
            if (_mousePos.x >= 0f && _mousePos.y >= 0f && _mousePos.x < Screen.width && _mousePos.y < Screen.height) // if the mouse is in the game window
            {
                float cycle = context.ReadValue<float>();

                CycleSlots((int)Mathf.Sign(cycle));

                UpdateInventoryUI();
            }
        }
    }

    private void CycleSlots(int _distance)
    {
        do
        {
            currentInventorySlot = (currentInventorySlot + _distance) % Inventory.Count;
            if (currentInventorySlot < 0) currentInventorySlot = Inventory.Count - 1;
        } while (Inventory[currentInventorySlot].Count <= 0);
    }

    private void CycleToNextItemOfType(ItemType _type)
    {
        for (int i = 0; i < Inventory.Count; i++)
        {
            CycleSlots(1);

            if (Inventory[currentInventorySlot].Item.Type == _type) break;
        }
    }

    public void UpdateInventoryUI()
    {
        tileInHandObj.sprite = Inventory[currentInventorySlot].Item.Icon;
        tileInHandObj.color = Inventory[currentInventorySlot].Count > 0 ? Color.white : colorIfStackEmpty;
    }

}


public class ItemStack
{

    public Item Item;
    public int Count;

    public ItemStack(Item _item, int _count)
    {
        Item = _item;
        Count = _count;
    }
}