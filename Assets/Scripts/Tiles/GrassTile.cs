using UnityEngine;

public class GrassTile : Tile
{

    public override void Init(Vector2Int pos)
    {
        //TileID = 1;

        // NEW BEHAVIOUR ON INIT????
        SetTileColor(pos.x, pos.y);
    }

}
