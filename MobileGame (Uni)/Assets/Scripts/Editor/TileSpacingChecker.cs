using UnityEngine;
using UnityEditor;

public class TileSpacingChecker : EditorWindow
{
    [MenuItem("Tools/Check Tile Spacing")]
    public static void CheckTileSpacing()
    {
        GridManager gridManager = GameObject.FindObjectOfType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("No GridManager found in scene!");
            return;
        }

        Tile[] tiles = gridManager.allTiles;

        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("No tiles found on GridManager!");
            return;
        }

        Debug.Log($"=== Tile Spacing Check ({tiles.Length} tiles) ===");

        // Log every tile position
        foreach (Tile tile in tiles)
        {
            Debug.Log($"Tile: {tile.name} | Position: {tile.transform.position}");
        }

        // Work out the spacing between the first two tiles as a reference
        if (tiles.Length < 2)
        {
            Debug.LogWarning("Need at least 2 tiles to check spacing.");
            return;
        }

        // Find the smallest X and Y distances between any two tiles
        // This gives us the cell size
        float smallestX = float.MaxValue;
        float smallestY = float.MaxValue;

        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = i + 1; j < tiles.Length; j++)
            {
                float dx = Mathf.Abs(tiles[i].transform.position.x - tiles[j].transform.position.x);
                float dy = Mathf.Abs(tiles[i].transform.position.y - tiles[j].transform.position.y);

                if (dx > 0.01f && dx < smallestX) smallestX = dx;
                if (dy > 0.01f && dy < smallestY) smallestY = dy;
            }
        }

        Debug.Log($"=== Detected Cell Size: X={smallestX:F4}  Y={smallestY:F4} ===");

        if (Mathf.Abs(smallestX - smallestY) > 0.01f)
        {
            Debug.LogWarning($"X and Y spacing are different! X={smallestX:F4} Y={smallestY:F4} — grid may not be square.");
        }
        else
        {
            Debug.Log($"Grid looks square. Cell size = {smallestX:F4}");
        }

        // Now flag any tiles that don't sit on the expected grid
        float cellSize = smallestX;
        Vector3 origin = tiles[0].transform.position;
        bool allClean = true;

        foreach (Tile tile in tiles)
        {
            float relX = tile.transform.position.x - origin.x;
            float relY = tile.transform.position.y - origin.y;

            float remX = relX % cellSize;
            float remY = relY % cellSize;

            // Allow a small tolerance
            bool xOk = remX < 0.05f || remX > cellSize - 0.05f;
            bool yOk = remY < 0.05f || remY > cellSize - 0.05f;

            if (!xOk || !yOk)
            {
                Debug.LogWarning($"MISALIGNED: {tile.name} at {tile.transform.position} | offX={remX:F4} offY={remY:F4}");
                allClean = false;
            }
        }

        if (allClean)
            Debug.Log("All tiles are evenly spaced!");
        else
            Debug.LogWarning("Some tiles are misaligned — run Tools/Snap Tiles To Grid to fix.");
    }

    [MenuItem("Tools/Check Tile Borders")]
    public static void CheckTileBorders()
    {
        GridManager gridManager = GameObject.FindObjectOfType<GridManager>();
        if (gridManager == null) { Debug.LogError("No GridManager found in scene!"); return; }

        Tile[] tiles = gridManager.allTiles;
        if (tiles == null || tiles.Length == 0) { Debug.LogError("No tiles found!"); return; }

        // Detect centre-to-centre spacing
        float smallestX = float.MaxValue;
        float smallestY = float.MaxValue;

        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = i + 1; j < tiles.Length; j++)
            {
                float dx = Mathf.Abs(tiles[i].transform.position.x - tiles[j].transform.position.x);
                float dy = Mathf.Abs(tiles[i].transform.position.y - tiles[j].transform.position.y);

                if (dx > 0.01f && dx < smallestX) smallestX = dx;
                if (dy > 0.01f && dy < smallestY) smallestY = dy;
            }
        }

        if (smallestX == float.MaxValue || smallestY == float.MaxValue)
        {
            Debug.LogError("Could not detect cell size — need at least 2 tiles.");
            return;
        }

        Debug.Log($"=== Tile Border/Gap Check ===");
        Debug.Log($"Centre-to-centre spacing: X={smallestX:F4}  Y={smallestY:F4}");

        bool allClean = true;
        float referenceGapX = -1f;
        float referenceGapY = -1f;

        foreach (Tile tile in tiles)
        {
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();

            if (sr == null)
            {
                Debug.LogWarning($"{tile.name} has no SpriteRenderer — skipping.");
                continue;
            }

            float tileWidth  = sr.bounds.size.x;
            float tileHeight = sr.bounds.size.y;
            float gapX = smallestX - tileWidth;
            float gapY = smallestY - tileHeight;

            // First valid tile sets the reference
            if (referenceGapX < 0) { referenceGapX = gapX; referenceGapY = gapY; }

            bool gapXOk = Mathf.Abs(gapX - referenceGapX) < 0.01f;
            bool gapYOk = Mathf.Abs(gapY - referenceGapY) < 0.01f;

            if (!gapXOk || !gapYOk)
            {
                Debug.LogWarning($"UNEVEN: {tile.name} | gapX={gapX:F4} (expected {referenceGapX:F4}) | gapY={gapY:F4} (expected {referenceGapY:F4})");
                allClean = false;
            }
            else
            {
                Debug.Log($"{tile.name} | size=({tileWidth:F4}, {tileHeight:F4}) | gap=({gapX:F4}, {gapY:F4}) ✓");
            }
        }

        if (referenceGapX >= 0)
            Debug.Log($"=== Gap between tiles: X={referenceGapX:F4}  Y={referenceGapY:F4} ===");

        if (allClean)
            Debug.Log("All tile gaps are even!");
        else
            Debug.LogWarning("Some gaps are uneven — check tile scales or sprite import sizes.");
    }

    [MenuItem("Tools/Snap Tiles To Grid")]
    public static void SnapTilesToGrid()
    {
        GridManager gridManager = GameObject.FindObjectOfType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("No GridManager found in scene!");
            return;
        }

        Tile[] tiles = gridManager.allTiles;

        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("No tiles found on GridManager!");
            return;
        }

        // Detect cell size the same way as the checker
        float smallestX = float.MaxValue;
        float smallestY = float.MaxValue;

        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = i + 1; j < tiles.Length; j++)
            {
                float dx = Mathf.Abs(tiles[i].transform.position.x - tiles[j].transform.position.x);
                float dy = Mathf.Abs(tiles[i].transform.position.y - tiles[j].transform.position.y);

                if (dx > 0.01f && dx < smallestX) smallestX = dx;
                if (dy > 0.01f && dy < smallestY) smallestY = dy;
            }
        }

        if (smallestX == float.MaxValue || smallestY == float.MaxValue)
        {
            Debug.LogError("Could not detect cell size — do you have at least 2 tiles?");
            return;
        }

        float cellSize = smallestX;

        // Use the tile with the most rounded position as the origin anchor
        Vector3 origin = tiles[0].transform.position;

        int snappedCount = 0;

        // Register undo so you can Ctrl+Z this in the editor
        Undo.RecordObjects(System.Array.ConvertAll(tiles, t => (Object)t.transform), "Snap Tiles To Grid");

        foreach (Tile tile in tiles)
        {
            Vector3 pos = tile.transform.position;

            // Snap each axis to the nearest multiple of cellSize from origin
            float snappedX = origin.x + Mathf.Round((pos.x - origin.x) / cellSize) * cellSize;
            float snappedY = origin.y + Mathf.Round((pos.y - origin.y) / cellSize) * cellSize;

            Vector3 snappedPos = new Vector3(snappedX, snappedY, pos.z);

            if (Vector3.Distance(pos, snappedPos) > 0.001f)
            {
                Debug.Log($"Snapped {tile.name}: {pos} → {snappedPos}");
                tile.transform.position = snappedPos;
                snappedCount++;
            }
        }

        // Mark the scene dirty so Unity knows to save the changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        if (snappedCount == 0)
            Debug.Log("All tiles were already aligned — nothing to snap.");
        else
            Debug.Log($"Snapped {snappedCount} tile(s) to grid. Cell size used: {cellSize:F4}. Press Ctrl+Z to undo.");
    }
}