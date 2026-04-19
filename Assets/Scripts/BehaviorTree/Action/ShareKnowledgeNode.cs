using UnityEngine;

public class ShareKnowledgeNode : Node
{
    private AgentBlackBoard bb;

    public ShareKnowledgeNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        if (bb.baseRef != null)
        {
            // This sends the Explorer's data into the FireBase's global "Memory"
            bb.baseRef.SyncKnowledge(bb);
        }

        // --- SEMESTER 2 RESET ---
        bb.fogTilesRevealed = 0; // Reset counter so we can go out again
        bb.currentTarget = null;
        bb.destinationObject = null;

        // Safely clear movement for both Semester 1 and Semester 2
        if (bb.mlBrain != null) bb.mlBrain.ClearTarget();
        if (bb.mover != null) bb.mover.ClearTarget();

        bb.ui?.SetState("Data Uploaded! Heading back out.");

        // Returning Success tells the Sequence we are DONE at the base.
        // The BT will now restart and pick a new exploration target.
        return NodeState.Success;
    }
}