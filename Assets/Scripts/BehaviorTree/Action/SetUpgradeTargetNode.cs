using UnityEngine;

public class SetUpgradeTargetNode : Node
{
    private AgentBlackBoard bb;

    public SetUpgradeTargetNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        CookingStation station = CookingStation.Instance;

        if (station == null)
        {
            bb.ui?.SetState("Upgrade Target Missing");
            return _state = NodeState.Failure;
        }

        // --- UNIVERSAL SETUP ---
        bb.currentTarget = station.transform;
        bb.destinationObject = station.transform; // <--- CRITICAL FOR ML-BRAIN

        // --- SEMESTER 1 PATHFINDING ---
        if (bb.mover != null)
        {
            bb.mover.SetTarget(station.transform);
        }

        bb.ui?.SetState("Targeting Cooking Station for Upgrade");
        return _state = NodeState.Success;
    }
}