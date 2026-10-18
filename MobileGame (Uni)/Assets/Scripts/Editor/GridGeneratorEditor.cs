using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

[CustomEditor(typeof(GridManager))]
public class GridGeneratorEditor : Editor
{
    private int columns = 5;
    private int rows    = 5;
    private float cellSize = 0.6868f;
    private float spacingX = 0.6868f;
    private float spacingY = 0.6868f;
    private GameObject tilePrefab;

    private void OnEnable()
    {
        GridManager gridManager = target as GridManager;
        if (gridManager == null)
        {
            return;
        }

        Tile[] tiles = gridManager.GetComponentsInChildren<Tile>();
        spacingX = GetSmallestAxisStep(tiles, true, spacingX);
        spacingY = GetSmallestAxisStep(tiles, false, spacingY);
    }

    public override void OnInspectorGUI()
    {
        // Draw the default GridManager inspector fields as normal
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("── Live Grid Spacing ──", EditorStyles.boldLabel);
        DrawLiveSpacingControls();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("── Grid Generator ──", EditorStyles.boldLabel);

        columns    = EditorGUILayout.IntField("Columns", columns);
        rows       = EditorGUILayout.IntField("Rows", rows);
        cellSize   = EditorGUILayout.FloatField("Cell Size", cellSize);
        tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        if (tilePrefab == null)
        {
            EditorGUILayout.HelpBox("Assign a Tile Prefab to generate the grid.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Generate Grid"))
            GenerateGrid();

        if (GUILayout.Button("Clear Grid"))
            ClearGrid();
    }

    private void DrawLiveSpacingControls()
    {
        GridManager gridManager = (GridManager)target;
        Tile[] tiles = gridManager.GetComponentsInChildren<Tile>();

        if (tiles.Length == 0)
        {
            EditorGUILayout.HelpBox("Generate a grid before adjusting its spacing.", MessageType.Info);
            return;
        }

        EditorGUI.BeginChangeCheck();
        float newSpacingX = EditorGUILayout.Slider("Horizontal", spacingX, 0.1f, 1.5f);
        if (EditorGUI.EndChangeCheck())
        {
            spacingX = newSpacingX;
            ApplySpacing(gridManager, tiles, true, spacingX);
        }

        EditorGUI.BeginChangeCheck();
        float newSpacingY = EditorGUILayout.Slider("Vertical", spacingY, 0.1f, 1.5f);
        if (EditorGUI.EndChangeCheck())
        {
            spacingY = newSpacingY;
            ApplySpacing(gridManager, tiles, false, spacingY);
        }

        EditorGUILayout.HelpBox("Adjust the sliders to move existing tiles. Undo is supported; this does not regenerate the grid.", MessageType.None);
    }

    private static void ApplySpacing(GridManager gridManager, Tile[] tiles, bool horizontal, float spacing)
    {
        List<float> axisLevels = GetAxisLevels(tiles, horizontal);
        if (axisLevels.Count == 0)
        {
            return;
        }

        List<Object> transforms = new List<Object>();
        foreach (Tile tile in tiles)
        {
            if (tile != null)
            {
                transforms.Add(tile.transform);
            }
        }

        Undo.RecordObjects(transforms.ToArray(), "Adjust Grid Spacing");

        foreach (Tile tile in tiles)
        {
            if (tile == null)
            {
                continue;
            }

            Vector3 position = tile.transform.position;
            float currentAxisPosition = horizontal ? position.x : position.y;
            int axisIndex = FindAxisIndex(axisLevels, currentAxisPosition);
            float centeredOffset = (axisIndex - (axisLevels.Count - 1) * 0.5f) * spacing;

            if (horizontal)
            {
                position.x = gridManager.transform.position.x + centeredOffset;
            }
            else
            {
                position.y = gridManager.transform.position.y + centeredOffset;
            }

            tile.transform.position = position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(tile.transform);
        }

        EditorSceneManager.MarkSceneDirty(gridManager.gameObject.scene);
    }

    private static float GetSmallestAxisStep(Tile[] tiles, bool horizontal, float fallback)
    {
        List<float> axisLevels = GetAxisLevels(tiles, horizontal);
        float smallestStep = float.MaxValue;

        for (int i = 1; i < axisLevels.Count; i++)
        {
            float step = axisLevels[i] - axisLevels[i - 1];
            if (step > 0.01f && step < smallestStep)
            {
                smallestStep = step;
            }
        }

        return smallestStep < float.MaxValue ? smallestStep : fallback;
    }

    private static List<float> GetAxisLevels(Tile[] tiles, bool horizontal)
    {
        List<float> axisLevels = new List<float>();

        foreach (Tile tile in tiles)
        {
            if (tile == null)
            {
                continue;
            }

            float axisPosition = horizontal ? tile.transform.position.x : tile.transform.position.y;
            bool alreadyTracked = false;
            foreach (float level in axisLevels)
            {
                if (Mathf.Abs(level - axisPosition) < 0.01f)
                {
                    alreadyTracked = true;
                    break;
                }
            }

            if (!alreadyTracked)
            {
                axisLevels.Add(axisPosition);
            }
        }

        axisLevels.Sort();
        return axisLevels;
    }

    private static int FindAxisIndex(List<float> axisLevels, float axisPosition)
    {
        for (int i = 0; i < axisLevels.Count; i++)
        {
            if (Mathf.Abs(axisLevels[i] - axisPosition) < 0.01f)
            {
                return i;
            }
        }

        return 0;
    }

    void GenerateGrid()
    {
        GridManager gridManager = (GridManager)target;

        // Clear existing tiles first
        ClearGrid();

        Tile[] newTiles = new Tile[columns * rows];
        int index = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                // Centre the grid around the GridManager's position
                float offsetX = x * cellSize - (columns - 1) * cellSize * 0.5f;
                float offsetY = y * cellSize - (rows - 1)    * cellSize * 0.5f;

                Vector3 pos = gridManager.transform.position + new Vector3(offsetX, offsetY, 0f);

                GameObject tileObj = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, gridManager.transform);
                tileObj.transform.position = pos;
                tileObj.name = $"Tile_{x}_{y}";

                Tile tile = tileObj.GetComponent<Tile>();
                if (tile == null)
                {
                    Debug.LogWarning($"Tile prefab is missing a Tile component on {tileObj.name}!");
                }
                else
                {
                    newTiles[index] = tile;
                }

                index++;
            }
        }

        // Assign the new tiles to GridManager
        gridManager.allTiles = newTiles;

        // Mark scene dirty so Unity saves the changes
        EditorUtility.SetDirty(gridManager);
        EditorSceneManager.MarkSceneDirty(gridManager.gameObject.scene);

        Debug.Log($"Generated {columns}x{rows} grid ({newTiles.Length} tiles) with cell size {cellSize}.");
    }

    void ClearGrid()
    {
        GridManager gridManager = (GridManager)target;

        // Destroy all child GameObjects (the old tiles)
        for (int i = gridManager.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(gridManager.transform.GetChild(i).gameObject);
        }

        gridManager.allTiles = new Tile[0];

        EditorUtility.SetDirty(gridManager);
        EditorSceneManager.MarkSceneDirty(gridManager.gameObject.scene);

        Debug.Log("Grid cleared.");
    }
}
