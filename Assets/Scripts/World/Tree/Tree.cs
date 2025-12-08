// File: Scripts/World/Tree/Tree.cs (Modified)

using UnityEngine;

public class Tree : MonoBehaviour, I_Interactable
{
    [Header("Data Reference")]
    public SO_TreeData data;

    public int currentWoodCapacity = 100;
    private GridManager gm;
    private SpriteRenderer sr;
    private TileData td;

    // NEW: Resource locking mechanism
    public GameObject claimedByAgent { get; private set; } = null; // Stores the GameObject of the agent currently claiming this tree

    public bool IsClaimed => claimedByAgent != null;

    /// <summary>
    /// Attempts to claim the tree. Returns true if successful (unclaimed or already claimed by agent).
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

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GridManager>();
        td = GetComponent<TileData>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (data == null)
        {
            Debug.LogWarning($"{name} has no TreeData assigned!");
            return;
        }

        currentWoodCapacity = data.maximumWoods;

        // Auto-apply sprite if exists in SO
        if (sr != null && data.woodsprites != null)
            sr.sprite = data.woodsprites[0];
    }

    public int HarvestWood(int amount)
    {
        int woodHarvested = Mathf.Min(amount, currentWoodCapacity);
        currentWoodCapacity -= woodHarvested;

        // Logic to change sprite based on wood capacity
        if (currentWoodCapacity <= 80 && currentWoodCapacity > 40)
        {
            // Change sprite to indicate current wood amount
            if (sr != null && data.woodsprites != null && data.woodsprites.Length >= 3)
                sr.sprite = data.woodsprites[1];
        }
        else if (currentWoodCapacity <= 40 && currentWoodCapacity > 0)
        {
            // Change sprite to indicate current wood amount
            if (sr != null && data.woodsprites != null && data.woodsprites.Length >= 3)
                sr.sprite = data.woodsprites[2];
        }

        if (currentWoodCapacity <= 0)
        {
            
            ReleaseClaim(claimedByAgent);
            gm.ReplaceTile(transform.position, gm.gridConfig.defaultTile);


        }

        return woodHarvested;
    }
    public void Interact(GameObject interactor)
    {
        Debug.Log($"Wood max capacity: {data.maximumWoods}" +
          $" Current capacity: {currentWoodCapacity}");

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
}