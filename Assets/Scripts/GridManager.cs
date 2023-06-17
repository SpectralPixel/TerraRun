using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{

    [Min(1)]
    public int GridWidth, GridHeight;

    [Space]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private List<Color> tileColors;

    private const string gridName = "Grid";

    private Dictionary<Vector2Int, Tile> tiles;

    private GameObject selectionObj;
    private Vector2Int selectedTile;

    private void Start()
    {
        selectionObj = Instantiate(tilePrefab.gameObject);
        selectionObj.name = "Selection";
        SpriteRenderer selectionRenderer = selectionObj.GetComponent<SpriteRenderer>();
        selectionRenderer.color = new Color(1f, 1f, 1f, 0.4f);
        selectionRenderer.sortingOrder = 1;

        GenerateGrid();
    }

    private void Update()
    {
        DrawSelection();
    }

    public void GenerateGrid()
    {
        GameObject oldGrid = GameObject.Find(gridName);
        if (oldGrid != null) DestroyImmediate(oldGrid);

        GameObject gridObject = new GameObject(gridName);

        tiles = new Dictionary<Vector2Int, Tile>();
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Tile newTile = Instantiate(tilePrefab);
                newTile.transform.parent = gridObject.transform;

                newTile.name = $"Tile {x} {y}";
                newTile.transform.position = new Vector3(x + 0.5f, y + 0.5f);

                tiles[new Vector2Int(x, y)] = newTile;

                newTile.Init(new Vector2(x, y), tileColors);
            }
        }
    }

    public Tile GetTileAtPos(Vector2Int pos)
    {
        if (tiles.TryGetValue(pos, out Tile tile)) return tile;
        return null;
    }

    public void DrawSelection()
    {
        Vector3 mouseCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        selectedTile = new Vector2Int(Mathf.FloorToInt(mouseCoords.x), Mathf.FloorToInt(mouseCoords.y));

        Debug.Log(selectedTile.ToString()); // DEBUG
        selectionObj.transform.position = new Vector3(selectedTile.x + 0.5f, selectedTile.y + 0.5f, 0f);
    }

}
