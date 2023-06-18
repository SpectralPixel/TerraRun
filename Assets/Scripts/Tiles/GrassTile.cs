using UnityEngine;

public class GrassTile : Tile
{

    public override void Init(Vector2Int pos)
    {
        TileID = 0;

        // NEW BEHAVIOUR ON INIT????
        SetTileColor(pos.x, pos.y);
    }

}
