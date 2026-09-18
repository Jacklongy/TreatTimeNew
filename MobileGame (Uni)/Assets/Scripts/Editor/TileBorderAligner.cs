using UnityEngine;
using UnityEditor;

public class TileBorderAligner : EditorWindow
{
    private GameObject tilePrefab;
    private float borderThickness = 0.05f;

    [MenuItem("Tools/Tile Border Aligner")]
    public static void ShowWindow()
    {
        GetWindow<TileBorderAligner>("Tile Border Aligner");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("── Tile Border Aligner ──", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        tilePrefab       = (GameObject)EditorGUILayout.ObjectField("Tile GameObject", tilePrefab, typeof(GameObject), true);
        borderThickness  = EditorGUILayout.FloatField("Border Thickness", borderThickness);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Assign your tile GameObject (in the scene). It must have a SpriteRenderer and 4 children named: BorderTop, BorderBottom, BorderLeft, BorderRight.", MessageType.Info);
        EditorGUILayout.Space();

        if (tilePrefab == null)
        {
            EditorGUILayout.HelpBox("Assign a Tile GameObject to continue.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Align Borders"))
            AlignBorders();
    }

    void AlignBorders()
    {
        // Get the parent sprite size
        SpriteRenderer parentSR = tilePrefab.GetComponent<SpriteRenderer>();
        if (parentSR == null)
        {
            Debug.LogError("Tile has no SpriteRenderer on the root — can't read size.");
            return;
        }

        float width  = parentSR.bounds.size.x;
        float height = parentSR.bounds.size.y;

        Debug.Log($"Tile size: {width:F4} x {height:F4} | Border thickness: {borderThickness:F4}");

        // Find each border child and position it
        // Top and Bottom — rotated 90 degrees to lay horizontal
        AlignBorder("BorderTop",    tilePrefab, new Vector3(0f, (height * 0.5f) + (borderThickness * 0.5f), 0f),  new Vector3(borderThickness, width + borderThickness * 2, 1f), 90f);
        AlignBorder("BorderBottom", tilePrefab, new Vector3(0f, -(height * 0.5f) - (borderThickness * 0.5f), 0f), new Vector3(borderThickness, width + borderThickness * 2, 1f), 90f);
        // Left and Right — stretch to cover full height including top/bottom border thickness
        AlignBorder("BorderLeft",   tilePrefab, new Vector3(-(width * 0.5f) - (borderThickness * 0.5f), 0f, 0f), new Vector3(borderThickness, height + borderThickness * 2, 1f), 0f);
        AlignBorder("BorderRight",  tilePrefab, new Vector3((width * 0.5f) + (borderThickness * 0.5f), 0f, 0f),  new Vector3(borderThickness, height + borderThickness * 2, 1f), 0f);

        // Mark dirty so Unity saves
        EditorUtility.SetDirty(tilePrefab);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(tilePrefab.scene);

        Debug.Log("Borders aligned successfully!");
    }

    void AlignBorder(string childName, GameObject parent, Vector3 localPosition, Vector3 localScale, float rotationZ = 0f)
    {
        Transform child = parent.transform.Find(childName);

        if (child == null)
        {
            Debug.LogWarning($"Could not find child named '{childName}' on {parent.name} — skipping.");
            return;
        }

        Undo.RecordObject(child, $"Align {childName}");

        child.localPosition = localPosition;
        child.localScale    = localScale;
        child.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

        EditorUtility.SetDirty(child);

        Debug.Log($"{childName} → position: {localPosition} | scale: {localScale} | rotZ: {rotationZ}");
    }
}