using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(WorldGenerator))]
public class GridManagerUtils : Editor
{

    public bool AutoUpdate = true;
    private int oldGridWidth, oldGridHeight, oldFloorHeight;
    private float oldNoiseMultiplier;
    private List<OctaveSetting> oldOctaves;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldGenerator generator = (WorldGenerator)target;

        AutoUpdate = GUILayout.Toggle(AutoUpdate, "Auto Update World");

        if (GUILayout.Button("Generate World")) generator.GenerateWorld();

        if (AutoUpdate)
        {
            if (oldGridWidth != generator.MapWidth ||
                oldGridHeight != generator.MapHeight ||
                oldFloorHeight != generator.floorHeight)
            {
                generator.GenerateWorld();
            }   
        }

        oldGridWidth = generator.MapWidth;
        oldGridHeight = generator.MapHeight;
        oldFloorHeight = generator.floorHeight;
        oldOctaves = generator.octaves;
    }
}