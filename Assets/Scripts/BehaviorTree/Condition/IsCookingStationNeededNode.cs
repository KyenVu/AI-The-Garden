using UnityEngine;

public class IsCookingStationNeededNode : Node
{
    private AgentBlackBoard bb;

    // We define BOTH costs here now to match the Build node!
    private const int LOW_FOOD_THRESHOLD = 10;
    private const int BUILD_COST_WOOD = 15;

    public IsCookingStationNeededNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        // --- Frame 1 Failsafe ---
        if (bb.baseRef == null)
        {
            bb.baseRef = GameObject.FindAnyObjectByType<FireBase>();
            if (bb.baseRef == null) return NodeState.Failure;
        }

        // Check 1: Does a Cooking Station already exist?
        if (CookingStation.Instance != null)
        {
            return NodeState.Failure;
        }

        // Check 2: Do we actually have enough Food AND Wood to pay for it?
        if (bb.baseRef.GetAmount(ResourceType.Food) < LOW_FOOD_THRESHOLD ||
            bb.baseRef.GetAmount(ResourceType.Wood) < BUILD_COST_WOOD)
        {
            return NodeState.Failure;
        }

        // If we have both, we can officially start building!
        bb.ui?.SetState("CRITICAL FOOD: Initiating Cooking Station Build.");
        return NodeState.Success;
    }
}