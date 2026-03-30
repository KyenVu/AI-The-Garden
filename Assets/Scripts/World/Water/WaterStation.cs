using UnityEngine;
using System.Collections.Generic;

public class WaterStation : MonoBehaviour, I_Interactable
{
    // Global reference for easy BT checking
    public static WaterStation Instance { get; private set; }

    [Header("Visuals")]
    public Sprite[] stationSprites;
    private SpriteRenderer sr;

    [Header("Production Status")]
    public int waterAvailable = 0;
    public int maxWaterAvailable = 5;
    public int waterOutput = 1;
    private float generateTimer = 0f;

    [Header("Upgrade State")]
    public int level = 1;
    public Dictionary<int, float> generateTimeByLevel = new Dictionary<int, float>
    {
        {1, 5f},
        {2, 3.5f},
        {3, 2.5f},
        {4, 1.5f}
    };
    public int maxLevel = 4;

    // Claiming system
    public GameObject claimedByAgent { get; private set; } = null;
    public bool IsClaimed => claimedByAgent != null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        sr = GetComponentInChildren<SpriteRenderer>();
        UpdateVisuals();
    }
    private void Start()
    {
        GridManager gm = FindObjectOfType<GridManager>();
        Collider2D col = GetComponent<Collider2D>();

        if (gm != null && col != null)
        {
            // Lock ALL tiles that fall under this building's collider
            List<TileData> tilesUnder = gm.GetTilesUnderCollider(col);
            foreach (TileData tile in tilesUnder)
            {
                tile.SetWalkableOverride(false);
            }
        }
    }
    void Update()
    {
        // Only generate water if capacity allows
        if (waterAvailable < maxWaterAvailable)
        {
            generateTimer += Time.deltaTime;
            if (generateTimer >= GetCurrentGenerateTime())
            {
                waterAvailable = Mathf.Min(waterAvailable + waterOutput, maxWaterAvailable);
                generateTimer = 0f;
            }
        }
    }

    private float GetCurrentGenerateTime()
    {
        if (generateTimeByLevel.ContainsKey(level)) return generateTimeByLevel[level];
        return generateTimeByLevel[1]; // Fallback
    }

    public bool TryClaim(GameObject agent)
    {
        if (IsClaimed && claimedByAgent != agent) return false;
        claimedByAgent = agent;
        return true;
    }

    public void ReleaseClaim(GameObject agent)
    {
        if (claimedByAgent == agent) claimedByAgent = null;
    }

    // Agent interacts with this to drink
    public int DrinkWater(AgentStatsManager stats)
    {
        if (waterAvailable <= 0) return 0;

        // Restore 30 hydration (adjust this value as needed to match your stats)
        stats.DrinkWater(30);
        waterAvailable -= waterOutput;

        return waterOutput;
    }

    private void UpdateVisuals()
    {
        if (sr == null || stationSprites == null || stationSprites.Length == 0) return;

        int spriteIndex = level - 1;

        if (spriteIndex >= 0 && spriteIndex < stationSprites.Length)
        {
            sr.sprite = stationSprites[spriteIndex];
        }
        else if (spriteIndex >= stationSprites.Length)
        {
            sr.sprite = stationSprites[stationSprites.Length - 1];
        }
    }

    public bool TryUpgrade()
    {
        if (level < maxLevel)
        {
            level++;
            UpdateVisuals();
            return true;
        }
        return false;
    }

    // I_Interactable implementation
    public void Interact(GameObject interactor) { }
    public void OnHoverEnter()
    {
        if (sr != null) sr.color = Color.cyan;
    }
    public void OnHoverExit()
    {
        if (sr != null) sr.color = Color.white;
    }
}