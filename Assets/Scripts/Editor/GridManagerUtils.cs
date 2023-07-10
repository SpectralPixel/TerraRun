using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGenerator))]
public class GridManagerUtils : Editor
{

    public bool AutoUpdate = true;
    private int oldSeed, oldGridWidth, oldGridHeight, oldFloorHeight;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldGenerator generator = (WorldGenerator)target;

        AutoUpdate = GUILayout.Toggle(AutoUpdate, "Auto Update World");

        if (GUILayout.Button("Generate World")) generator.GenerateWorld();

        if (AutoUpdate)
        {
            if (oldSeed != generator.WorldSeed ||
                oldGridWidth != generator.MapWidth ||
                oldGridHeight != generator.MapHeight ||
                oldFloorHeight != generator.FloorHeight)
            {
                generator.GenerateWorld();
            }   
        }

        oldSeed = generator.WorldSeed;
        oldGridWidth = generator.MapWidth;
        oldGridHeight = generator.MapHeight;
        oldFloorHeight = generator.FloorHeight;
    }
}