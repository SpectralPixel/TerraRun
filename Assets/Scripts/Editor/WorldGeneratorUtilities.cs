using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldGenerator))]
public class WorldGeneratorUtilities : Editor
{

    private bool AutoUpdate = true;
    private int previewResolution;

    private int oldSeed, oldFloorHeight, oldFloorSmoothing;
    private Vector2Int oldMapPreviewSize;
    private WorldGenerator worldGenerator;
    private GridManager gridManager;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (WorldGenerator.Instance == null)
        {
            FindObjectOfType<WorldGenerator>().EnsureSingleton();
        }
        worldGenerator = WorldGenerator.Instance;

        if (GridManager.Instance == null)
        {
            FindObjectOfType<GridManager>().EnsureSingleton();
        }
        gridManager = GridManager.Instance;

        previewResolution = (int)Mathf.Sqrt(worldGenerator.PreviewMapResolution);

        previewResolution = EditorGUILayout.IntSlider("Preview Resolution", previewResolution, 1, 4);
        worldGenerator.PreviewMapResolution = (int)Mathf.Pow(previewResolution, 2f);

        worldGenerator.PreviewMapSize = EditorGUILayout.Vector2IntField("Preview Size", worldGenerator.PreviewMapSize);
        worldGenerator.PreviewMapSize.x = Mathf.Clamp(worldGenerator.PreviewMapSize.x, 1, 1024);
        worldGenerator.PreviewMapSize.y = Mathf.Clamp(worldGenerator.PreviewMapSize.y, 1, 1024);

        worldGenerator.ConservePreviewXSize = GUILayout.Toggle(worldGenerator.ConservePreviewXSize, "Conserve Preview X Size?");
        worldGenerator.ConservePreviewYSize = GUILayout.Toggle(worldGenerator.ConservePreviewYSize, "Conserve Preview Y Size?");

        if (worldGenerator.PreviewMapSize.x * worldGenerator.PreviewMapSize.y <= 5000)
        {
            AutoUpdate = GUILayout.Toggle(AutoUpdate, "Auto Update Preview?");
        }
        else AutoUpdate = false;

        if (GUILayout.Button("Generate World"))
        {
            GenerateWorld();
        }

        if (AutoUpdate && !Application.isPlaying)
        {
            if (oldSeed != worldGenerator.WorldSeed ||
                oldMapPreviewSize != worldGenerator.PreviewMapSize ||
                oldFloorHeight != worldGenerator.FloorHeight ||
                oldFloorSmoothing != worldGenerator.FloorSmoothing)
            {
                GenerateWorld();
            }   
        }

        oldSeed = worldGenerator.WorldSeed;
        oldMapPreviewSize = worldGenerator.PreviewMapSize;
        oldFloorHeight = worldGenerator.FloorHeight;
        oldFloorSmoothing = worldGenerator.FloorSmoothing;
    }

    private void GenerateWorld()
    {
        GameUtilities.InitializeUtilities();

        worldGenerator.InitializeWorld();
        gridManager.InitializeGrid();

        worldGenerator.GeneratePreviewWorld();
        gridManager.CreateWorldPreview();
    }

}