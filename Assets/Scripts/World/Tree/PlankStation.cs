using UnityEngine;
using System.Collections.Generic;

public class PlankStation : MonoBehaviour, I_Interactable
{
    public static PlankStation Instance { get; private set; }

    [Header("Visuals")]
    public Sprite[] stationSprites;
    private SpriteRenderer sr;

    [Header("Storage")]
    public float storedWood = 0f;

    [Header("Upgrade State")]
    public int level = 1;
    public int maxLevel = 3;
    public Dictionary<int, float> multiplierByLevel = new Dictionary<int, float>
    {
        {1, 1.5f},
        {2, 2.5f},
        {3, 3.5f}
    };

    // Tracking the tiles this building currently sits on
    private List<TileData> lockedTiles = new List<TileData>();

    public GameObject claimedByAgent { get; private set; } = null;
    public bool IsClaimed => claimedByAgent != null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        sr = GetComponentInChildren<SpriteRenderer>();
        UpdateVisuals();
    }

    void Start()
    {
        // 1. Calculate the initial footprint and lock the tiles
        UpdateStationFootprint();

        // 2. THE BIG TRANSFER: Move all existing wood from the Base to here!
        FireBase baseRef = FindObjectOfType<FireBase>();
        if (baseRef != null)
        {
            int baseWood = baseRef.GetAmount(ResourceType.Wood);
            if (baseWood > 0)
            {
                baseRef.RemoveResource(ResourceType.Wood, baseWood);
                storedWood += baseWood;
                Debug.Log($"Transferred {baseWood} raw wood from Base to Plank Station!");
            }
        }

        // Just in case the base transferred more than 70 wood right at the start!
        CheckForAutoUpgrade();
    }

    private void UpdateStationFootprint()
    {
        if (sr == null || sr.sprite == null) return;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = sr.sprite.bounds.size;
            Physics2D.SyncTransforms();
        }

        GridManager gm = FindObjectOfType<GridManager>();
        if (gm != null && col != null)
        {
            foreach (TileData oldTile in lockedTiles)
            {
                if (oldTile != null) oldTile.SetWalkableOverride(true);
            }
            lockedTiles.Clear();

            lockedTiles = gm.GetTilesUnderCollider(col);
            foreach (TileData newTile in lockedTiles)
            {
                if (newTile != null) newTile.SetWalkableOverride(false);
            }
        }
    }

    public void DepositWood(int rawWoodAmount, AgentBlackBoard bb = null)
    {
        float multiplier = multiplierByLevel.ContainsKey(level) ? multiplierByLevel[level] : 1.5f;
        storedWood += (rawWoodAmount * multiplier);

        // NEW: Sync knowledge if a blackboard is provided
        if (bb != null && bb.baseRef != null)
        {
            bb.baseRef.SyncKnowledge(bb);
        }

        CheckForAutoUpgrade();
    }

    // ==========================================
    // --- NEW: AUTOMATIC UPGRADE LOGIC ---
    // ==========================================
    private void CheckForAutoUpgrade()
    {
        // We use a while loop just in case an agent drops off a massive amount of wood 
        // (e.g., 200 wood) so it can instantly jump from Level 1 -> Level 3 in one go!
        bool upgraded = true;
        while (upgraded)
        {
            upgraded = false;

            if (level == 1 && GetWoodCount() >= 70)
            {
                storedWood -= 70; // Deduct the cost
                TryUpgrade();
                Debug.Log("Plank Station automatically upgraded to Level 2!");
                upgraded = true;
            }
            else if (level == 2 && GetWoodCount() >= 90)
            {
                storedWood -= 90; // Deduct the cost
                TryUpgrade();
                Debug.Log("Plank Station automatically upgraded to Level 3 (Max)!");
                upgraded = true;
            }
        }
    }
    // ==========================================

    public int GetWoodCount()
    {
        return Mathf.FloorToInt(storedWood);
    }

    public bool RemoveWood(int amount)
    {
        if (GetWoodCount() >= amount)
        {
            storedWood -= amount;
            return true;
        }
        return false;
    }

    private void UpdateVisuals()
    {
        if (sr == null || stationSprites == null || stationSprites.Length == 0) return;

        int spriteIndex = level - 1;
        if (spriteIndex >= 0 && spriteIndex < stationSprites.Length)
            sr.sprite = stationSprites[spriteIndex];
        else if (spriteIndex >= stationSprites.Length)
            sr.sprite = stationSprites[stationSprites.Length - 1];
    }

    public bool TryUpgrade()
    {
        if (level < maxLevel)
        {
            level++;
            UpdateVisuals();
            UpdateStationFootprint();
            return true;
        }
        return false;
    }

    public bool TryClaim(GameObject agent)
    {
        if (IsClaimed && claimedByAgent != agent) return false;
        claimedByAgent = agent;
        return true;
    }

    public void ReleaseClaim(GameObject agent) { if (claimedByAgent == agent) claimedByAgent = null; }

    public void Interact(GameObject interactor) { }
    public void OnHoverEnter() { if (sr != null) sr.color = Color.yellow; }
    public void OnHoverExit() { if (sr != null) sr.color = Color.white; }
}