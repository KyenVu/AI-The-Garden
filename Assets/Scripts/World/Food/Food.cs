using UnityEngine;

public class Food : MonoBehaviour, I_Interactable
{
    public SO_FoodData data;

    private GridManager gm;

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
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log($"{data.foodName} clicked by {interactor?.name ?? "Player"}");
    }

    public void OnHoverEnter() => GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
    public void OnHoverExit() => GetComponentInChildren<SpriteRenderer>().color = Color.white;

    public void OnEaten()
    {
        if (data == null)
        {
            Debug.LogWarning($"{name} has no FoodData assigned!");
            return;
        }
        ReleaseClaim(claimedByAgent);
        Debug.Log($"{data.foodName} was eaten, restoring {data.nutritionValue} hunger.");
        gm.ReplaceTile(transform.position, gm.gridConfig.defaultTile);

        // Optional: Play eat sound
        if (data.eatSound != null)
            AudioSource.PlayClipAtPoint(data.eatSound, transform.position);
    }

}
