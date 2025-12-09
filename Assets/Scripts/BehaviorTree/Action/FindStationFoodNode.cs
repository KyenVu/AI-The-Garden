// File: Scripts/BehaviorTree/Action/FindStationFoodNode.cs (New File)

using UnityEngine;

public class FindStationFoodNode : Node
{
    private AgentBlackBoard bb;

    public FindStationFoodNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        CookingStation station = CookingStation.Instance;

        // 1. Check if the station exists, has food, and is claimable
        if (station != null && station.cookedFoodAvailable > 0)
        {
            if (station.TryClaim(bb.mover.gameObject))
            {
                bb.currentTarget = station.transform;
                bb.mover.SetTarget(station.transform);
                bb.ui?.SetState($"Hungry! Moving to Cooking Station (Lv.{station.level})");
                return _state = NodeState.Success;
            }
        }

        // No suitable station found or station claimed
        return _state = NodeState.Failure;
    }
}