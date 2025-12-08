using UnityEngine;

public class WaterSource : MonoBehaviour, I_Interactable
{
    [Header("Data Reference")]
    public SO_WaterSourceData data;

    public float currentWaterCapacity;
    private GridManager gm;
    private SpriteRenderer sr;

    // NEW: Resource locking mechanism
    public GameObject claimedByAgent { get; private set; } = null; // Stores the GameObject of the agent currently claiming this food
    public bool IsClaimed => claimedByAgent != null;

    /// <summary>
    /// Attempts to claim the food. Returns true if successful (unclaimed or already claimed by agent).
    /// </summary>
    public bool TryClaim(GameObject agent)
    {
        if (IsClaimed && claimedByAgent != agent)
            return false; // Already claimed by another agent

        claimedByAgent = agent;
        return true; // Claim successful
    }

    /// <summary>
    /// Releases the claim if it was held by the given agent.
    /// </summary>
    public void ReleaseClaim(GameObject agent)
    {
        if (claimedByAgent == agent)
        {
            claimedByAgent = null;
        }
    }

    private void Start()
    {
        gm = FindObjectOfType<GridManager>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (data == null)
        {
            Debug.LogWarning($"{name} has no WaterSourceData assigned!");
            return;
        }

        currentWaterCapacity = data.capacity;

        // Auto-apply sprite if exists in SO
        if (sr != null && data.waterSprite != null)
            sr.sprite = data.waterSprite;
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log($"Water max capacity: {data.capacity}" +
            $" Current capacity: {currentWaterCapacity}");

    }

    public void OnHoverEnter()
    {
        if (sr != null)
            sr.color = Color.cyan;
    }

    public void OnHoverExit()
    {
        if (sr != null)
            sr.color = Color.white;
    }
    public int Drink(int drinkRate)
    {
        if (data == null) return 0;

        int amount = data.hydrationValue;
        currentWaterCapacity -= drinkRate;

        if (data.drinkSound != null)
            AudioSource.PlayClipAtPoint(data.drinkSound, transform.position);

        // If empty  replace tile with default tile
        if (currentWaterCapacity <= 0)
        {
            Debug.Log($"{name} water source DEPLETED. Replacing with default tile.");
            gm.ReplaceTile(transform.position, gm.gridConfig.defaultTile);
            ReleaseClaim(claimedByAgent); // Ensure claim is released if depleted
        }

        return amount;
    }

    /// <summary>
    /// Percentage for UI or visual effects.
    /// </summary>
    public float GetFillPercent()
    {
        return Mathf.Clamp01(currentWaterCapacity / data.capacity);
    }
}
