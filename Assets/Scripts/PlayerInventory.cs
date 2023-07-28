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
        // add starting inventory???

        UpdateInventoryUI();
    }

    public ItemStack GetCurrentStack()
    {
        return Inventory[currentInventorySlot];
    }

    public void AddItems(Item newItem, int amount)
    {
        foreach (ItemStack stack in Inventory)
        {
            if (stack.Item.ItemID == newItem.ItemID)
            {
                stack.Count += amount;
                UpdateInventoryUI();
                return;
            }
        }

        Inventory.Add(new ItemStack(newItem, amount));
    }

    public void RemoveItems(Item newItem, int amount)
    {
        foreach (ItemStack stack in Inventory)
        {
            if (stack.Item.ItemID == newItem.ItemID)
            {
                stack.Count -= amount;
                if (stack.Count < 0) stack.Count = 0;
                UpdateInventoryUI();
                return;
            }
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

                if (cycle > 0f) // if switching to the next item
                {
                    do
                    {
                        currentInventorySlot = (currentInventorySlot + 1) % Inventory.Count;
                    } while (Inventory[currentInventorySlot].Count <= 0);
                }
                else
                {
                    do
                    {
                        currentInventorySlot--;
                        if (currentInventorySlot < 0) currentInventorySlot = Inventory.Count - 1;
                    } while (Inventory[currentInventorySlot].Count <= 0);
                }

                UpdateInventoryUI();
            }
        }
    }

    public void UpdateInventoryUI()
    {
        tileInHandObj.sprite = Inventory[currentInventorySlot].Item.Icon;
        tileInHandObj.color = Inventory[currentInventorySlot].Count > 0 ? Color.white : colorIfStackEmpty;
    }

}


[System.Serializable]
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