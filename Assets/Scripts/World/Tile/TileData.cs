// File: Scripts/World/Tile/TileData.cs (Modified)

using UnityEngine;

public class TileData : MonoBehaviour
{
    public int x, y;
    public TileType tileType;
    public SpriteRenderer spriteRenderer;

    // NEW: Local override flag (default is true, meaning we defer to tileType.walkable)
    private bool _isWalkableOverride = true;

    // MODIFIED: Walkable property checks the local override first
    public bool Walkable => _isWalkableOverride && tileType != null && tileType.walkable;

    public void Init(int x, int y, TileType type)
    {
        this.x = x;
        this.y = y;
        tileType = type;

        // Ensure override is true on init
        _isWalkableOverride = true;

        // Get SpriteRenderer (root or child)
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // === Set sprite ===
        if (spriteRenderer != null && tileType != null)
        {
            spriteRenderer.sprite = tileType.sprite;
            spriteRenderer.transform.localScale = Vector3.one * tileType.size;

            //  Apply sorting layer and order
            spriteRenderer.sortingLayerName = tileType.sortingLayer;
            spriteRenderer.sortingOrder = tileType.orderInLayer;
        }

        // === Set physics layer ===
        if (!string.IsNullOrEmpty(tileType.physicsLayer))
        {
            int layerIndex = LayerMask.NameToLayer(tileType.physicsLayer);
            if (layerIndex == -1)
            {
                Debug.LogWarning($"[TileData] Physics layer '{tileType.physicsLayer}' not found in project settings!");
            }
            else
            {
                gameObject.layer = layerIndex;
            }
        }

        if (tileType != null && tileType.addCollider)
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = gameObject.AddComponent<BoxCollider2D>();

            collider.isTrigger = tileType.isTrigger;

            if (spriteRenderer != null && spriteRenderer.sprite != null)
                collider.size = spriteRenderer.sprite.bounds.size * tileType.size;
        }


        gameObject.name = $"Tile ({x},{y}) - {(tileType ? tileType.id : "None")}";
    }

    /// <summary>
    /// NEW: Allows external components (like Tree) to mark this tile as unwalkable, overriding the TileType.
    /// </summary>
    public void SetWalkableOverride(bool isWalkable)
    {
        _isWalkableOverride = isWalkable;
    }
}