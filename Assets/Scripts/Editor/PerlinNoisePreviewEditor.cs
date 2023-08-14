using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerlinNoisePreview))]
public class PerlinNoisePreviewEditor : Editor
{

    private PerlinNoisePreview perlinNoisePreview;

    private int oldTextureSize;
    private float oldScale;
    private Vector2 oldOffset;
    private float oldMinimumThreshold;
    private float oldMaximumThreshold;


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        perlinNoisePreview = (PerlinNoisePreview)target;

        if (perlinNoisePreview.MinimumThreshold > perlinNoisePreview.MaximumThreshold) perlinNoisePreview.MinimumThreshold = perlinNoisePreview.MaximumThreshold;

        perlinNoisePreview.AutoUpdate = GUILayout.Toggle(perlinNoisePreview.AutoUpdate, "Auto Update Noise?");

        if (GUILayout.Button("Generate Perlin Noise"))
        {
            GenerateNoise();
        }

        if (perlinNoisePreview.AutoUpdate && (
            oldTextureSize != perlinNoisePreview.TextureSize ||
            oldScale != perlinNoisePreview.Scale ||
            oldOffset != perlinNoisePreview.Offset ||
            oldMinimumThreshold != perlinNoisePreview.MinimumThreshold ||
            oldMaximumThreshold != perlinNoisePreview.MaximumThreshold
            ))
        {
            GenerateNoise();
        }

        oldTextureSize = perlinNoisePreview.TextureSize;
        oldScale = perlinNoisePreview.Scale;
        oldOffset = perlinNoisePreview.Offset;
        oldMinimumThreshold = perlinNoisePreview.MinimumThreshold;
        oldMaximumThreshold = perlinNoisePreview.MaximumThreshold;
    }

    private void GenerateNoise()
    {
        perlinNoisePreview.GeneratePerlinNoise();
    }
}