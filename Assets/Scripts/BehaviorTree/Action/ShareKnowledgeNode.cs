using UnityEngine;

public class ShareKnowledgeNode : Node
{
    private AgentBlackBoard bb;

    public ShareKnowledgeNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        if (bb.baseRef != null)
        {
            // Trigger the Sync!
            bb.baseRef.SyncKnowledge(bb);
        }

        // Reset exploration counters
        bb.fogTilesRevealed = 0;
        bb.currentTarget = null;
        bb.mover.ClearTarget();

        bb.ui?.SetState("Data Uploaded! Heading back out.");
        return NodeState.Success;
    }
}