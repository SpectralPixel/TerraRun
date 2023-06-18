using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerHand : MonoBehaviour
{

    [SerializeField] private GameObject selectionObj;

    private Vector2Int selectedTile;

    private void Start()
    {
        selectionObj = Instantiate(selectionObj);
        selectionObj.name = "Selection";
        SpriteRenderer selectionRenderer = selectionObj.GetComponent<SpriteRenderer>();
        selectionRenderer.color = new Color(1f, 1f, 1f, 0.4f);
        selectionRenderer.sortingOrder = 1;
    }

    private void Update()
    {
        TileSelection();

        if (Input.GetMouseButtonDown(0))
        {
            GridManager.WorldTiles.TryGetValue(selectedTile, out Tile tile);
            if (tile != null) TileClicked(true);
        }

        if (Input.GetMouseButtonDown(1))
        {
            GridManager.WorldTiles.TryGetValue(selectedTile, out Tile tile);
            if (tile != null) TileClicked(false);
        }
    }

    public void TileSelection()
    {
        Vector3 mouseCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        selectedTile = new Vector2Int(Mathf.FloorToInt(mouseCoords.x), Mathf.FloorToInt(mouseCoords.y));

        selectionObj.transform.position = new Vector3(selectedTile.x + 0.5f, selectedTile.y + 0.5f, 0f);
    }

    void TileClicked(bool mine = true)
    {
        Tile clickedTile = GridManager.WorldTiles[selectedTile];
        Debug.Log(clickedTile.name);

        if (mine)
        {
            int curTileIndex = clickedTile.TileID;
            Debug.Log(curTileIndex.ToString());
            if (curTileIndex + 1 < GridManager.AllTiles.Count) GridManager.UpdateGrid(selectedTile, GridManager.AllTiles[curTileIndex + 1]);
        }
        else
        {
            int curTileIndex = clickedTile.TileID;
            Debug.Log(curTileIndex.ToString());
            if (curTileIndex - 1 >= 0) GridManager.UpdateGrid(selectedTile, GridManager.AllTiles[curTileIndex - 1]);
        }
    }
}
