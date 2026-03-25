using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class LargeObstacle : MonoBehaviour
{
    private List<TileData> coveredTiles = new List<TileData>();
    private BoxCollider2D boxCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        RegisterObstacle();
    }

    private void RegisterObstacle()
    {
        GridManager grid = FindAnyObjectByType<GridManager>();
        if (grid == null) return;

        // 1. Get the exact bounds of the collider
        Vector2 center = (Vector2)transform.position + boxCollider.offset;
        Vector2 size = boxCollider.size * transform.localScale;

        // 2. Find all colliders inside this area (on the Tile layer)
        // Adjust the LayerMask to match your Tile layer
        int tileLayer = LayerMask.GetMask("Default");
        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0f, tileLayer);

        foreach (var col in colliders)
        {
            TileData tile = col.GetComponent<TileData>();
            if (tile != null)
            {
                // 3. Mark the tile as blocked and store it for later cleanup
                tile.SetWalkableOverride(false);
                coveredTiles.Add(tile);
                Debug.Log($"Large Obstacle {name} blocked tile ({tile.x}, {tile.y})");
            }
        }
    }

    private void OnDestroy()
    {
        // 4. When the large object is removed, free all tiles it covered
        foreach (TileData tile in coveredTiles)
        {
            if (tile != null)
                tile.SetWalkableOverride(true);
        }
    }
}