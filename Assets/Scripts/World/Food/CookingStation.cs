using UnityEngine;
using System.Collections.Generic;

public class CookingStation : MonoBehaviour, I_Interactable
{
    // Global reference for easy checking in the Behavior Tree (BT)
    public static CookingStation Instance { get; private set; }
    [Header("Visuals")]
    public Sprite[] stationSprites;
    private SpriteRenderer sr; // Reference to the renderer

    [Header("Production Status")]
    public int cookedFoodAvailable = 0;
    public int maxFoodAvailable = 5;    
    public int foodOutput = 1;          
    private float cookTimer = 0f;

    [Header("Upgrade State")]
    public int level = 1;
    public Dictionary<int, float> cookTimeByLevel = new Dictionary<int, float>
    {
        {1, 5f},
        {2, 3.5f},
        {3, 2.5f},
        {4, 1.5f}
    };
    public int maxLevel = 4;

    // Claiming system (omitted for brevity)
    public GameObject claimedByAgent { get; private set; } = null;
    public bool IsClaimed => claimedByAgent != null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        // NEW: Get the SpriteRenderer reference on Awake
        sr = GetComponentInChildren<SpriteRenderer>();
        UpdateVisuals(); // Set initial sprite
    }

    void Update()
    {
        // Only cook if capacity allows
        if (cookedFoodAvailable < maxFoodAvailable)
        {
            cookTimer += Time.deltaTime;
            if (cookTimer >= GetCurrentCookTime())
            {
                cookedFoodAvailable = Mathf.Min(cookedFoodAvailable + foodOutput, maxFoodAvailable);
                cookTimer = 0f;
            }
        }
    }

    private float GetCurrentCookTime()
    {
        if (cookTimeByLevel.ContainsKey(level)) return cookTimeByLevel[level];
        return cookTimeByLevel[1]; // Fallback to Level 1 time
    }

    // Used by agent finder/action nodes
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

    public int GatherFood(AgentStatsManager stats)
    {
       
        stats.EatFood(30); 
        if (cookedFoodAvailable <= 0) return 0;
        cookedFoodAvailable -= foodOutput;
        return foodOutput;
    }

    private void UpdateVisuals()
    {
        if (sr == null || stationSprites == null || stationSprites.Length == 0) return;

        // Ensure the level index is valid (Level 1 is index 0)
        int spriteIndex = level - 1;

        if (spriteIndex >= 0 && spriteIndex < stationSprites.Length)
        {
            sr.sprite = stationSprites[spriteIndex];
            
        }
        else if (spriteIndex >= stationSprites.Length)
        {
            // If we are at max level but array is shorter, use the last sprite
            sr.sprite = stationSprites[stationSprites.Length - 1];
        }
    }

    public bool TryUpgrade()
    {
        if (level < maxLevel)
        {
            level++;
            UpdateVisuals(); // CRUCIAL: Update the sprite after level change!
            Debug.Log($"Cooking Station upgraded to Level {level} (Cook Time: {GetCurrentCookTime()}s)");
            return true;
        }
        return false;
    }
    // I_Interactable implementation (minimal for now)
    public void Interact(GameObject interactor) => Debug.Log($"Cooking Station Level: {level}");
    public void OnHoverEnter() => GetComponentInChildren<SpriteRenderer>().color = Color.magenta;
    public void OnHoverExit() => GetComponentInChildren<SpriteRenderer>().color = Color.white;
}