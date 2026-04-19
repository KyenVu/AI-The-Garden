using UnityEngine;

public class IsWaterStationNeededNode : Node
{
    private AgentBlackBoard bb;

    // Must match the costs in BuildWaterStationNode!
    private const int BUILD_COST_WATER = 10;
    private const int BUILD_COST_WOOD = 15;

    public IsWaterStationNeededNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        // --- THE FIX: Frame 1 Base Failsafe ---
        if (bb.baseRef == null)
        {
            bb.baseRef = GameObject.FindAnyObjectByType<FireBase>();
            if (bb.baseRef == null) return NodeState.Failure;
        }

        // If the station is already built, we don't need to build it again
        if (WaterStation.Instance != null)
        {
            return _state = NodeState.Failure;
        }

        // If we don't have enough resources in the base, we can't build it yet
        if (bb.baseRef.GetAmount(ResourceType.Water) < BUILD_COST_WATER ||
            bb.baseRef.GetAmount(ResourceType.Wood) < BUILD_COST_WOOD)
        {
            return _state = NodeState.Failure;
        }

        // We need it, and we can afford it!
        return _state = NodeState.Success;
    }
}