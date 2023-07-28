using System.Collections.Generic;
using Unity.VisualScripting;
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
            if ((tile == null || tile.Type == TileType.Gas || tile.Type == TileType.Background) && inventory.GetCurrentStack().Item.Type == ItemType.Tool)
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

                int directionFromPlayer = (int)Mathf.Sign(transform.position.x - mouseCoords.x);

                List<Vector2Int> directionsToCheck = new List<Vector2Int>()
                {
                    new Vector2Int(directionFromPlayer, 0),
                    Vector2Int.down,
                    Vector2Int.up,
                    new Vector2Int(-directionFromPlayer, 0),
                };

                bool _tileFound = false;
                foreach (Vector2Int direction in directionsToCheck)
                {
                    GridManager.WorldTiles.TryGetValue(selectedTile + direction, out tile);
                    if (tile != null && tile.Type != TileType.Gas && tile.Type != TileType.Background)
                    {
                        selectedTile += direction;
                        _tileFound = true;
                        break;
                    }
                }

                if (!_tileFound)
                {
                    float _shortestDistance = Mathf.Infinity;
                    Vector2Int _closestTile = selectedTile;
                    for (int i = 0; i < 5 && _shortestDistance == Mathf.Infinity; i++) // search within a 25 tile radius (if no tile has been found)
                    {
                        for (int x = -5 * i; x < 5 * i; x++)
                        {
                            for (int y = -5 * i; y < 5 * i; y++)
                            {
                                if (i > 0 && x >= -5 * i - 1 && x <= 5 * i - 1 && y >= -5 * i - 1 && y <= 5 * i - 1) // if we should've already have checked this tile
                                {
                                    Vector2Int _pos = selectedTile + new Vector2Int(x, y);

                                    GridManager.WorldTiles.TryGetValue(_pos, out Tile _tile);
                                    if (_tile != null && _tile.Type != TileType.Gas && _tile.Type != TileType.Background)
                                    {
                                        float distance = Mathf.Sqrt(x * x + y * y);
                                        if (distance < _shortestDistance)
                                        {
                                            _shortestDistance = distance;
                                            _closestTile = _pos;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    selectedTile = _closestTile;
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
                    GridManager.UpdateGrid(selectedTile, GameUtilities.ItemToTile(inventory.GetCurrentStack().Item));
                    inventory.RemoveItems(inventory.GetCurrentStack().Item, 1);
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
