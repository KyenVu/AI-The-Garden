using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

[DisallowMultipleComponent]
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public GridConfig gridConfig;              // ScriptableObject with width, height, seed, palette
    public GameObject tilePrefab;              // Prefab with TileData
    public Transform gridParent;               // Parent to hold all tiles

    [Header("Debug")]
    public bool autoGenerateOnStart = true;
    public bool showGizmos = true;

    [Header("Spawned Object Data")]
    public SO_FoodData defaultFoodData;
    public SO_WaterSourceData defaultWaterData;
    public SO_TreeData defaultTreeData;
    public AnimatorController fireplaceAnimatorController;
    private Animator fireplaceAnimator;

    [Header("Base Camp Settings")]
    public TileType fireplaceTileType;     // assign your Fireplace TileType
    public GameObject fireplacePrefab;     // optional - visual prefab (fire, storage, etc.)
    public Transform baseTransform;        // runtime reference for agents
    [ColorUsage(true, true)] public Color fireLightColor = new Color(1f, 0.55f, 0.2f, 1f);
    [Range(0f, 5f)] public float fireLightIntensity = 2f;
    [Range(1f, 20f)] public float fireLightRadius = 5f;
    [Range(1f, 180f)] public float fireSpotAngle = 90f;


    private TileData[,] tiles;
    private Random rng;
    public bool IsGridReady { get; private set; }

    public event Action<int, int> OnGridBuilt;
    public event Action<TileData> OnTileSpawned;

    private void Start()
    {
        if (autoGenerateOnStart && gridConfig != null)
        {
            BuildGrid(gridConfig);
        }
    }

    /// <summary>
    /// Builds the grid deterministically based on GridConfig + seed.
    /// REFACTORED: Consolidated resource setup using helper methods.
    /// </summary>
    public void BuildGrid(GridConfig config)
    {
        if (config == null)
        {
            Debug.LogError(" GridConfig not assigned!");
            return;
        }

        ClearGrid();

        rng = new Random(config.seed);
        int width = Mathf.Max(1, config.width);
        int height = Mathf.Max(1, config.height);
        tiles = new TileData[width, height];

        // Generate grid
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create tile
                GameObject tileGO = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, gridParent);
                tileGO.name = $"Tile ({x},{y})";
                TileData td = tileGO.GetComponent<TileData>();

                // Assign a random TileType based on weights
                TileType chosen = ChooseTileType(config);
                td.Init(x, y, chosen);

                // --- CONSOLIDATED RESOURCE TILE HANDLING ---
                SetupResourceTile(tileGO, td, chosen);
                // -------------------------------------------

                tiles[x, y] = td;

                OnTileSpawned?.Invoke(td);
            }
        }

        IsGridReady = true;
        OnGridBuilt?.Invoke(width, height);
        Debug.Log($" Grid built {width}x{height} using seed {config.seed}");

        // === Place Fireplace at the Center ===
        int centerX = width / 2;
        int centerY = height / 2;

        if (fireplaceTileType != null)
        {
            Debug.Log($"[GridManager] Placing fireplace at ({centerX},{centerY})");

            // Destroy whatever tile was there
            if (tiles[centerX, centerY] != null)
            {
                Destroy(tiles[centerX, centerY].gameObject);
                tiles[centerX, centerY] = null;
            }

            // Instantiate a new fireplace tile
            GameObject fireTileGO = Instantiate(tilePrefab, new Vector3(centerX, centerY, 0), Quaternion.identity, gridParent);
            fireTileGO.AddComponent<FireBase>();
            var fireBaseLight = fireTileGO.AddComponent<Light2D>();
            fireBaseLight.lightType = Light2D.LightType.Point;
            fireBaseLight.color = fireLightColor;
            fireBaseLight.intensity = fireLightIntensity;
            fireBaseLight.pointLightOuterRadius = fireLightRadius;
            fireBaseLight.pointLightInnerRadius = fireLightRadius * 0.5f;

            fireplaceAnimator = fireTileGO.AddComponent<Animator>();
            fireplaceAnimator.runtimeAnimatorController = fireplaceAnimatorController;
            TileData fireTile = fireTileGO.GetComponent<TileData>();

            fireTile.Init(centerX, centerY, fireplaceTileType);

            // Store it
            tiles[centerX, centerY] = fireTile;
            OnTileSpawned?.Invoke(fireTile);
        }
    }

    // -------------------------------------------------------------------------
    // NEW HELPER METHODS FOR RESOURCE SETUP
    // -------------------------------------------------------------------------

    /// <summary>
    /// Dispatches resource setup based on the TileType ID.
    /// </summary>
    private void SetupResourceTile(GameObject tileGO, TileData td, TileType chosen)
    {
        if (chosen == null) return;

        if (chosen.id.Equals("Food", StringComparison.OrdinalIgnoreCase))
        {
            // Use ?? to add component if missing
            Food food = tileGO.GetComponent<Food>() ?? tileGO.AddComponent<Food>();
            food.data = defaultFoodData;
            // Food does not use the tile size multiplier for its collider size
            SetupResourceCollider(tileGO, td, 1f);
        }
        else if (chosen.id.Equals("Water", StringComparison.OrdinalIgnoreCase))
        {
            WaterSource water = tileGO.GetComponent<WaterSource>() ?? tileGO.AddComponent<WaterSource>();
            water.data = defaultWaterData;
            // Water uses the tile size multiplier
            SetupResourceCollider(tileGO, td, chosen.size);
        }
        else if (chosen.id.Equals("Tree", StringComparison.OrdinalIgnoreCase))
        {
            Tree tree = tileGO.GetComponent<Tree>() ?? tileGO.AddComponent<Tree>();
            tree.data = defaultTreeData;
            // Tree uses the tile size multiplier
            SetupResourceCollider(tileGO, td, chosen.size);
        }
    }

    /// <summary>
    /// Creates and configures the BoxCollider2D for resource tiles.
    /// </summary>
    private void SetupResourceCollider(GameObject tileGO, TileData td, float sizeMultiplier)
    {
        // FIX: Replaced null-coalescing assignment with explicit GetComponent/AddComponent check
        // This ensures the component is reliably present before we try to access its properties.
        BoxCollider2D col = tileGO.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = tileGO.AddComponent<BoxCollider2D>();
        }

        if (col != null)
        {
            col.isTrigger = true;

            if (td.spriteRenderer != null && td.spriteRenderer.sprite != null)
            {
                // The size logic is now centralized and uses the sizeMultiplier parameter
                col.size = td.spriteRenderer.sprite.bounds.size * sizeMultiplier;
            }
        }
        else
        {
            Debug.LogError($"[GridManager] Failed to add BoxCollider2D to {tileGO.name}");
        }
    }

    // -------------------------------------------------------------------------
    // EXISTING UTILITY METHODS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the world position of the base (fireplace).
    /// If the base doesn't exist yet, it safely returns the grid center instead.
    /// </summary>
    public Vector3 GetBaseLocation()
    {
        //if (baseTransform != null)
        //    return baseTransform.position;

        // Fallback — use grid center
        if (tiles != null)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);
            return new Vector3(width / 2f, height / 2f, 0f);
        }

        // Absolute fallback — origin
        return Vector3.zero;
    }


    /// <summary>
    /// Chooses a TileType based on weighted probabilities in GridConfig.
    /// </summary>
    private TileType ChooseTileType(GridConfig cfg)
    {
        if (cfg.tilePalette == null || cfg.tilePalette.Count == 0)
            return cfg.defaultTile;

        float roll = (float)rng.NextDouble();
        float cumulative = 0f;

        foreach (var entry in cfg.tilePalette)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
                return entry.tileType;
        }

        // fallback
        return cfg.defaultTile;
    }

    /// <summary>
    /// Returns tile at grid coordinate (x, y).
    /// </summary>
    public TileData GetTileAt(int x, int y)
    {
        if (tiles == null) return null;
        if (x < 0 || y < 0 || x >= tiles.GetLength(0) || y >= tiles.GetLength(1))
            return null;
        return tiles[x, y];
    }

    /// <summary>
    /// Returns neighboring walkable tiles.
    /// </summary>
    public List<TileData> GetNeighbors(TileData tile, bool diagonals = false)
    {
        List<TileData> result = new List<TileData>();
        int[,] dirs4 = new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
        int[,] dirs8 = new int[,]
        {
            { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 },
            { 1, 1 }, { -1, -1 }, { -1, 1 }, { 1, -1 }
        };

        int[,] dirs = diagonals ? dirs8 : dirs4;

        for (int i = 0; i < dirs.GetLength(0); i++)
        {
            int nx = tile.x + dirs[i, 0];
            int ny = tile.y + dirs[i, 1];
            TileData n = GetTileAt(nx, ny);
            if (n != null && n.Walkable)
                result.Add(n);
        }

        return result;
    }
    /// <summary>
    /// Returns the movement cost between two tiles.
    /// </summary>
    public float MoveCost(TileData from, TileData to)
    {
        if (to == null || !to.Walkable)
            return float.PositiveInfinity; // not walkable = impossible to move

        // Optional: terrain-based cost modifier
        if (to.tileType != null)
            return Mathf.Max(0.1f, to.tileType.moveCost);

        // Default cost if no data
        return 1f;
    }


    public TileData GetTileAtWorldPosition(Vector3 worldPos)
    {
        if (tiles == null) return null;

        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);
        return GetTileAt(x, y);
    }

    /// <summary>
    /// Returns the closest TileData from a list of candidate tiles to a world position.
    /// </summary>
    public TileData GetClosestTile(Vector3 fromWorldPos, List<TileData> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return GetTileAtWorldPosition(fromWorldPos); // fallback to nearest tile

        TileData best = null;
        float bestDist = float.MaxValue;

        foreach (var t in candidates)
        {
            if (t == null) continue;
            Vector3 world = new Vector3(t.x, t.y, 0);
            float d = (world - fromWorldPos).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        // fallback to nearest tile in grid if none chosen
        return best ?? GetTileAtWorldPosition(fromWorldPos);
    }

    /// <summary>
    /// Returns all tiles whose positions fall inside a collider's bounds.
    /// </summary>
    public List<TileData> GetTilesUnderCollider(Collider2D col)
    {
        if (col == null) return new List<TileData>();
        return GetTilesWithinBounds(col.bounds);
    }
    /// <summary>
    /// Returns all tiles whose positions are inside the given Bounds.
    /// </summary>
    public List<TileData> GetTilesWithinBounds(Bounds bounds)
    {
        var results = new List<TileData>();
        if (tiles == null) return results;

        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                TileData t = tiles[x, y];
                if (t == null) continue;
                if (bounds.Contains(new Vector3(t.x, t.y, 0)))
                    results.Add(t);
            }
        }

        return results;
    }

    /// <summary>
    /// Returns a random walkable tile position.
    /// </summary>
    public TileData GetRandomWalkableTile()
    {
        if (tiles == null) return null;
        for (int i = 0; i < 100; i++) // safety loop
        {
            int x = rng.Next(0, tiles.GetLength(0));
            int y = rng.Next(0, tiles.GetLength(1));
            if (tiles[x, y] != null && tiles[x, y].Walkable)
                return tiles[x, y];
        }
        return null;
    }

    /// <summary>
    /// Replaces the tile at a world position with a new tile prefab and TileType.
    /// If the old tile object was destroyed, it safely instantiates a new one.
    /// </summary>
    public void ReplaceTile(Vector3 worldPos, TileType newType)
    {
        if (newType == null)
        {
            Debug.LogWarning("[GridManager] ReplaceTile called with null TileType!");
            return;
        }

        // Snap to grid coordinates
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        // Make sure we're inside grid bounds
        if (tiles == null || x < 0 || y < 0 || x >= tiles.GetLength(0) || y >= tiles.GetLength(1))
        {
            Debug.LogWarning($"[GridManager] ReplaceTile out of bounds: ({x},{y})");
            return;
        }

        TileData oldTile = tiles[x, y];

        // Destroy the old tile GameObject if it still exists
        if (oldTile != null)
        {
            Destroy(oldTile.gameObject);
            tiles[x, y] = null;
        }

        // Instantiate a fresh tile prefab
        GameObject newTileGO = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, gridParent);
        TileData newTile = newTileGO.GetComponent<TileData>();

        // Initialize with new tile type data
        newTile.Init(x, y, newType);

        // Store it in the grid array
        tiles[x, y] = newTile;

        OnTileSpawned?.Invoke(newTile);
        Debug.Log($"[GridManager] Replaced tile at ({x},{y}) with new {newType.id}");
    }



    /// <summary>
    /// Clears all generated tiles.
    /// </summary>
    public void ClearGrid()
    {
        if (gridParent != null)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
                DestroyImmediate(gridParent.GetChild(i).gameObject);
        }
        tiles = null;
        IsGridReady = false;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || tiles == null) return;
        Gizmos.color = Color.white;
        foreach (var t in tiles)
        {
            if (t == null) continue;
            Gizmos.color = t.Walkable ? Color.white : Color.red;
            Gizmos.DrawWireCube(new Vector3(t.x, t.y, 0), Vector3.one * 0.9f);
        }
    }
}