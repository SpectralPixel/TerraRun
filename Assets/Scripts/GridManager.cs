using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    [Min(1)]
    public int GridWidth, GridHeight;

    [SerializeField] private List<Tile> allTiles;
    [SerializeField] private GameObject selectionObj;

    private const string gridName = "Grid";

    private Dictionary<Vector2Int, Tile> tileMap;

    private Vector2Int selectedTile;

    private void Start()
    {
        selectionObj = Instantiate(selectionObj);
        selectionObj.name = "Selection";
        SpriteRenderer selectionRenderer = selectionObj.GetComponent<SpriteRenderer>();
        selectionRenderer.color = new Color(1f, 1f, 1f, 0.4f);
        selectionRenderer.sortingOrder = 1;

        GenerateGrid();
    }

    private void Update()
    {
        TileSelection();

        if (Input.GetMouseButtonDown(0))
        {
            tileMap.TryGetValue(selectedTile, out Tile tile);
            if (tile != null) TileClicked(true);
        }

        if (Input.GetMouseButtonDown(1))
        {
            tileMap.TryGetValue(selectedTile, out Tile tile);
            if (tile != null) TileClicked(false);
        }
    }

    public void GenerateGrid()
    {
        GameObject oldGrid = GameObject.Find(gridName);
        if (oldGrid != null) DestroyImmediate(oldGrid);

        GameObject gridObject = new GameObject(gridName);

        tileMap = new Dictionary<Vector2Int, Tile>();
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                CreateTileObject(x, y, allTiles[0]);
            }
        }
    }

    public void UpdateGrid(Vector2Int pos, Tile tile)
    {
        Destroy(GameObject.Find($"/Grid/Tile {pos.x} {pos.y}"));
        CreateTileObject(pos.x, pos.y, tile);
    }

    void CreateTileObject(int x, int y, Tile tile)
    {
        Tile newTile = Instantiate(tile);
        newTile.transform.parent = GameObject.Find("/Grid").transform;

        newTile.name = $"Tile {x} {y}";
        newTile.transform.position = new Vector3(x + 0.5f, y + 0.5f);

        tileMap[new Vector2Int(x, y)] = newTile;

        newTile.Init(new Vector2Int(x, y));
    }

    void TileClicked(bool mine = true)
    {
        Tile clickedTile = tileMap[selectedTile];
        Debug.Log(clickedTile.name);

        if (mine)
        {
            int curTileIndex = clickedTile.TileID;
            Debug.Log(curTileIndex.ToString());
            if (curTileIndex + 1 < allTiles.Count) UpdateGrid(selectedTile, allTiles[curTileIndex + 1]);
        }
        else
        {
            int curTileIndex = clickedTile.TileID;
            Debug.Log(curTileIndex.ToString());
            if (curTileIndex - 1 >= 0) UpdateGrid(selectedTile, allTiles[curTileIndex - 1]);
        }
    }

    public Tile GetTileAtPos(Vector2Int pos)
    {
        if (tileMap.TryGetValue(pos, out Tile tile)) return tile;
        return null;
    }

    public void TileSelection()
    {
        Vector3 mouseCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        selectedTile = new Vector2Int(Mathf.FloorToInt(mouseCoords.x), Mathf.FloorToInt(mouseCoords.y));

        selectionObj.transform.position = new Vector3(selectedTile.x + 0.5f, selectedTile.y + 0.5f, 0f);
    }

}
