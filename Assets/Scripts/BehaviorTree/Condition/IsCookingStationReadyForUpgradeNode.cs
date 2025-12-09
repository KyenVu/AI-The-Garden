// File: Scripts/BehaviorTree/Condition/IsCookingStationReadyForUpgradeNode.cs (New File)
using UnityEngine;

public class IsCookingStationReadyForUpgradeNode : Node
{
    private AgentBlackBoard bb;
    // Food required at base to trigger the upgrade (Lv2, Lv3, Lv4)
    private readonly int[] UPGRADE_THRESHOLDS = { 15, 20, 25 };

    public IsCookingStationReadyForUpgradeNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        FireBase fb = bb.baseRef;
        CookingStation station = CookingStation.Instance;

        if (station == null || fb == null)
            return _state = NodeState.Failure;

        // Check 1: Station must not be max level
        if (station.level >= station.maxLevel)
        {
            bb.ui?.SetState("Station Max Level");
            return _state = NodeState.Failure;
        }

        // Determine the required food threshold for the next level
        int targetLevelIndex = station.level; // Level 1 is index 1, needs threshold at index 0 (15)

        if (targetLevelIndex > UPGRADE_THRESHOLDS.Length)
        {
            bb.ui?.SetState("Invalid Upgrade Level");
            return _state = NodeState.Failure;
        }

        int requiredFood = UPGRADE_THRESHOLDS[targetLevelIndex - 1]; // Use level-1 for array index

        // Check 2: Does the base have enough food?
        if (fb.GetAmount(ResourceType.Food) >= requiredFood)
        {
            bb.ui?.SetState($"Ready for Lv.{station.level + 1} upgrade ({requiredFood} food needed).");
            return _state = NodeState.Success;
        }

        return _state = NodeState.Failure;
    }
}