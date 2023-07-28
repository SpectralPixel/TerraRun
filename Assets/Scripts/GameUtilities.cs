using System.Collections.Generic;
using UnityEngine;

public static class GameUtilities
{

    // IN THE FUTURE MAKE LISTS OF ITEMS AND TILES BE PULLED FROM FILE FOLDERS
    public static Dictionary<string, Tile> AllTiles;
    public static Dictionary<string, Item> AllItems;
    public static Dictionary<string, string> ItemDropConversions; // when an item is dropped, the player gets another item instead (e.g. player breaks generated wood, gets placeable wood)
    public static List<Vector2Int> CheckDirections;

    public static void SetupVariables()
    {
        CheckDirections = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        ItemDropConversions = new Dictionary<string, string>()
        {
            { "WoodTile", "AltWoodTile" },
            { "GrassTile", "DirtTile" },
        };
    }

    public static Item TileToItem(Tile _tile)
    {
        ItemDropConversions.TryGetValue(_tile.TileID, out string _newID);
        if (_newID != null) return AllItems[_newID];
        return AllItems[_tile.TileID];
    }

    public static Tile ItemToTile(Item _item)
    {
        return AllTiles[_item.ItemID];
    }

}
