using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;

public class PlayerHand : MonoBehaviour
{

    [SerializeField] private GameObject selectionObj;
    [SerializeField] private Image tileInHandObj;

    private PlayerInventory inventory;

    private Vector2Int selectedTile;
    private int itemInHand;

    private void Start()
    {
        inventory = GetComponent<PlayerInventory>();

        selectionObj = Instantiate(selectionObj);
        selectionObj.name = "Selection";
        SpriteRenderer selectionRenderer = selectionObj.GetComponent<SpriteRenderer>();
        selectionRenderer.color = new Color(1f, 1f, 1f, 0.4f);
        selectionRenderer.sortingOrder = 1;
    }

    private void Update()
    {
        TileSelection();
    }

    public void TileSelection()
    {
        Vector3 mouseCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        selectedTile = new Vector2Int(Mathf.FloorToInt(mouseCoords.x), Mathf.FloorToInt(mouseCoords.y));

        selectionObj.transform.position = new Vector3(selectedTile.x + 0.5f, selectedTile.y + 0.5f, 0f);
    }

    void TileClicked()
    {
        ItemType itemType = inventory.Inventory[itemInHand].Type;
        string itemID = inventory.Inventory[itemInHand].ItemID;
        switch (itemType)
        {
            case ItemType.Weapon:
                break;
            case ItemType.Tool:
                GridManager.UpdateGrid(selectedTile);
                break;
            case ItemType.Consumable:
                break;
            case ItemType.Tile:
                GridManager.UpdateGrid(selectedTile, GridManager.AllTiles[itemID]);
                break;
        }
    }

    public void Use(InputAction.CallbackContext context)
    {
        if (context.started) TileClicked();
    }

    public void CycleHotbar(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            float cycle = context.ReadValue<float>();

            if (cycle > 0f) // if switching to the next item
            {
                itemInHand = (itemInHand + 1) % inventory.Inventory.Count;
            }
            else
            {
                itemInHand--;
                if (itemInHand < 0) itemInHand = inventory.Inventory.Count - 1;
            }

            Debug.Log(itemInHand.ToString());
            tileInHandObj.sprite = inventory.Inventory[itemInHand].Icon;
        }
    }
}
