using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(GridManager))]
public class GridGeneratorEditor : Editor
{
    private int columns = 5;
    private int rows    = 5;
    private float cellSize = 0.6868f;
    private GameObject tilePrefab;

    public override void OnInspectorGUI()
    {
        // Draw the default GridManager inspector fields as normal
        DrawDefaultInspector();

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
