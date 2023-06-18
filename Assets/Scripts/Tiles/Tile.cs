using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{

    [SerializeField] protected List<Color> tileColors;

    public int TileID = -1;

    public virtual void Init(Vector2Int pos)
    {
        SetTileColor(pos.x, pos.y);
    }

    protected void SetTileColor(int x, int y)
    {
        for (int i = 0; i < tileColors.Count; i++)
        {
            if ((x + y) % tileColors.Count == i)
            {
                GetComponent<SpriteRenderer>().color = tileColors[i];
            }
        }
    }
}
