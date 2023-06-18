using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGenerator))]
public class GridManagerUtils : Editor
{

    public bool AutoUpdate = true;
    private int oldGridWidth, oldGridHeight;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldGenerator manager = (WorldGenerator)target;

        AutoUpdate = GUILayout.Toggle(AutoUpdate, "Auto Update World");

        if (GUILayout.Button("Generate World")) manager.GenerateWorld();

        if (AutoUpdate)
        {
            if (oldGridWidth != manager.MapWidth || oldGridHeight != manager.MapHeight)
                manager.GenerateWorld();
        }

        oldGridWidth = manager.MapWidth;
        oldGridHeight = manager.MapHeight;
    }
}