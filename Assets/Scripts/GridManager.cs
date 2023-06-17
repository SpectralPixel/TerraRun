using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    [Min(1)]
    public int GridWidth, GridHeight;

    [Space]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private List<Color> tileColors;

    private const string gridName = "Grid";

    private Vector2Int selectedTile;

    public void GenerateGrid()
    {
        GameObject oldGrid = GameObject.Find(gridName);
        if (oldGrid != null) DestroyImmediate(oldGrid);

        GameObject gridObject = new GameObject(gridName);

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Tile newTile = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);
                newTile.name = $"Tile {x} {y}";
                newTile.transform.parent = gridObject.transform;

                newTile.Init(new Vector2(x, y), tileColors);
            }
        }
    }

    public void DrawSelection()
    {
        Vector3 mouseCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        selectedTile = new Vector2Int(Mathf.FloorToInt(mouseCoords.x), Mathf.FloorToInt(mouseCoords.y));

        Debug.Log(selectedTile.ToString()); // DEBUG
    }

    private void Update()
    {
        DrawSelection();
    }

}
