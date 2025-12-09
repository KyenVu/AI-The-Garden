// File: Scripts/BehaviorTree/Condition/IsCookingStationNeededNode.cs (New File)
using UnityEngine;

public class IsCookingStationNeededNode : Node
{
    private AgentBlackBoard bb;
    private const int LOW_FOOD_THRESHOLD = 10;

    public IsCookingStationNeededNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        FireBase fb = bb.baseRef;
        if (fb == null) return _state = NodeState.Failure;

        // Check 1: Is Food supply enough to warrant a station? (>= 10)
        bool foodIsEnough = fb.GetAmount(ResourceType.Food) >= LOW_FOOD_THRESHOLD;

        // Check 2: Does a Cooking Station already exist?
        bool stationExists = CookingStation.Instance != null;

        if (foodIsEnough && !stationExists)
        {
            bb.ui?.SetState("CRITICAL FOOD: Initiating Cooking Station Build.");
            return _state = NodeState.Success;
        }

        return _state = NodeState.Failure;
    }
}