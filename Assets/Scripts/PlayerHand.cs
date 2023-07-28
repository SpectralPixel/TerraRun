using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHand : MonoBehaviour
{

    [SerializeField] private GameObject selectionObj;

    private PlayerInventory inventory;

    private Vector2Int selectedTile;
    private Vector2Int oldSelectedTile;

    private float timeClicked;

    private bool mouseDown;
    private bool smartCursor;

    private void Start()
    {
        inventory = GetComponent<PlayerInventory>();

        selectionObj = Instantiate(selectionObj);
        selectionObj.name = "Selection";
        Destroy(selectionObj.GetComponent<BoxCollider2D>());
        SpriteRenderer selectionRenderer = selectionObj.GetComponent<SpriteRenderer>();
        selectionRenderer.color = new Color(1f, 1f, 1f, 0.4f);
        selectionRenderer.sortingOrder = 1;
    }

    private void Update()
    {
        TileSelection();
        if (oldSelectedTile != selectedTile) timeClicked = 0f;

        if (mouseDown)
        {
            TileClicked();
            timeClicked += Time.deltaTime;
        }

        oldSelectedTile = selectedTile;
    }

    public void TileSelection()
    {
        Vector3 mouseCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (smartCursor)
        {
            selectedTile = new Vector2Int(Mathf.FloorToInt(mouseCoords.x), Mathf.FloorToInt(mouseCoords.y));

            GridManager.WorldTiles.TryGetValue(selectedTile, out Tile tile);
            if ((tile == null || tile.Type == TileType.Gas) && inventory.GetCurrentStack().Item.Type == ItemType.Tool)
            {
                /*
                 * Basic implementation of smart cursor.
                 * 
                 * All four neighboring tiles are checked in the order of "directionsToCheck" to see if they can be mined.
                 * If the checked tile can be mined, select it.
                 * 
                 * The tile that is closest to the player on the x-axis is prioritized to make tunnelling downwards easy.
                 * The tile furthest to the player on the x-axis has the lowest priority to aid when tunnelling sideways.
                 */

                List<Vector2Int> directionsToCheck = new List<Vector2Int>()
                {
                    Vector2Int.down,
                    Vector2Int.up,
                    new Vector2Int(1, 1),
                    new Vector2Int(-1, 1),
                    new Vector2Int(-1, -1),
                    new Vector2Int(1, -1),
                };

                int directionFromPlayer = (int)Mathf.Sign(transform.position.x - mouseCoords.x);
                directionsToCheck.Insert(0, new Vector2Int(directionFromPlayer, 0));
                directionsToCheck.Insert(3, new Vector2Int(-directionFromPlayer, 0));

                foreach (Vector2Int direction in directionsToCheck)
                {
                    GridManager.WorldTiles.TryGetValue(selectedTile + direction, out tile);
                    if (tile != null && tile.Type != TileType.Gas)
                    {
                        selectedTile += direction;
                        break;
                    }
                }
            }

            selectionObj.transform.position = new Vector3(selectedTile.x + 0.5f, selectedTile.y + 0.5f);
        }
        else
        {
            selectedTile = new Vector2Int(Mathf.FloorToInt(mouseCoords.x), Mathf.FloorToInt(mouseCoords.y));
            selectionObj.transform.position = new Vector3(selectedTile.x + 0.5f, selectedTile.y + 0.5f, 0f);
        }
    }

    void TileClicked()
    {
        Item curItem = inventory.GetCurrentStack().Item;
        ItemType itemType = curItem.Type;
        switch (itemType)
        {
            case ItemType.Weapon:
                break;
            case ItemType.Tool:
                GridManager.WorldTiles.TryGetValue(selectedTile, out Tile _tileToAdd);
                if (_tileToAdd != null && timeClicked * curItem.Power > _tileToAdd.Hardness)
                {
                    if (_tileToAdd.Type == TileType.Solid)
                    {
                        inventory.AddItems(GameUtilities.TileToItem(_tileToAdd), 1);
                        GridManager.UpdateGrid(selectedTile, GameUtilities.AllTiles["AirTile"]);
                    }
                    else if (_tileToAdd.Type == TileType.Tree)
                    {
                        GridManager.Trees.TryGetValue(selectedTile, out Tree _tree);
                        for (int i = 0; i < 8; i++)
                        {
                            GridManager.Trees.TryGetValue(new Vector2Int(selectedTile.x, selectedTile.y - i), out _tree);
                            if (_tree != null) break;
                        }
                        if (_tree != null && timeClicked * curItem.Power > _tileToAdd.Hardness * _tree.TreeHeight)
                        {
                            Debug.Log((_tileToAdd.Hardness * _tree.TreeHeight).ToString());
                            inventory.AddItems(GameUtilities.TileToItem(_tileToAdd), _tree.TreeHeight);
                            _tree.DestroyTree();
                        }
                    }
                }
                break;
            case ItemType.Consumable:
                break;
            case ItemType.Tile:
                GridManager.WorldTiles.TryGetValue(selectedTile, out Tile _tile);
                if ((_tile == null || _tile.Type == TileType.Gas) && inventory.GetCurrentStack().Count > 0)
                {
                    inventory.RemoveItems(inventory.GetCurrentStack().Item, 1);
                    GridManager.UpdateGrid(selectedTile, GameUtilities.ItemToTile(inventory.GetCurrentStack().Item));
                }
                break;
        }
    }

    public void Use(InputAction.CallbackContext context)
    {
        if (context.started) mouseDown = true;
        if (context.canceled) mouseDown = false;
    }

    public void ToggleSmartCursor(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            smartCursor = !smartCursor;
        }
    }
}
