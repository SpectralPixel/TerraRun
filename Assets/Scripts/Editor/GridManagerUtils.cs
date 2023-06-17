using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerUtils : Editor
{

    public bool AutoUpdate;
    private int oldGridWidth, oldGridHeight;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridManager manager = (GridManager)target;

        AutoUpdate = GUILayout.Toggle(AutoUpdate, "Auto Update Grid");

        if (GUILayout.Button("Create Grid")) manager.GenerateGrid();

        if (AutoUpdate)
        {
            if (oldGridWidth != manager.GridWidth || oldGridHeight != manager.GridHeight)
                manager.GenerateGrid();
        }

        oldGridWidth = manager.GridWidth;
        oldGridHeight = manager.GridHeight;
    }
}