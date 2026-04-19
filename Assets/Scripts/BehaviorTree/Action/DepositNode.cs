using UnityEngine;

public class DepositNode : Node
{
    private AgentBlackBoard bb;

    public DepositNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        bb.baseRef.SyncKnowledge(bb);

        // --- UNIVERSAL ARRIVAL CHECK ---
        bool isMoving = bb.mlBrain != null ? false : (bb.mover != null && !bb.mover.HasReachedDestination());

        if (isMoving)
        {
            bb.ui?.SetState("Moving to deposit location...");
            return _state = NodeState.Running;
        }

        foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
        {
            int have = bb.inventory.GetAmount(t);
            if (have <= 0) continue;

            int moved = bb.inventory.TransferTo(bb.baseRef, t, have);
            if (moved > 0) bb.ui?.SetState($"Deposited {moved} {t}");
        }

        if (bb.mover != null) bb.mover.ClearTarget();
        if (bb.mlBrain != null) bb.mlBrain.ClearTarget();
        return _state = NodeState.Success;
    }
}