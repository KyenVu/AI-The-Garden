// DepositNode.cs
using UnityEngine;

public class DepositNode : Node
{
    private AgentBlackBoard bb;

    public DepositNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        bb.baseRef.SyncKnowledge(bb);

        if (!bb.mover.HasReachedDestination())
        {
            bb.ui?.SetState("Moving to deposit location...");
            return _state = NodeState.Running;
        }

        // We completely removed the check for "currentTarget.GetComponent<FireBase>()"
        // Whether they walked to the Plank Station or the Fire Base, we process the 
        // transaction through bb.baseRef because it acts as our central Proxy Router!

        foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
        {
            int have = bb.inventory.GetAmount(t);
            if (have <= 0) continue;

            // bb.baseRef will auto-detect if the resource is Wood and forward it 
            // to PlankStation.Instance.DepositWood(amount) behind the scenes!
            int moved = bb.inventory.TransferTo(bb.baseRef, t, have);
            if (moved > 0)
            {
                bb.ui?.SetState($"Deposited {moved} {t}");
            }
        }

        // Safely clear the target so they can get their next task
        bb.mover.ClearTarget();
        return _state = NodeState.Success;
    }
}