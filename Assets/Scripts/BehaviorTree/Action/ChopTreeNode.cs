using UnityEngine;

public class ChopTreeNode : Node
{
    private AgentBlackBoard bb;
    private float chopTime = 3f;
    private float timer = 0f;

    public ChopTreeNode(AgentBlackBoard bb)
    {
        this.bb = bb;
    }

    public override NodeState Evaluate()
    {
        Tree tree = bb.currentTarget?.GetComponent<Tree>();

        // --- UNIVERSAL SETUP ---
        GameObject agentObj = bb.mlBrain != null ? bb.mlBrain.gameObject : bb.mover.gameObject;
        bool isMoving = bb.mlBrain != null ? false : (bb.mover != null && !bb.mover.HasReachedDestination());

        // FAIL if target is invalid, is not a tree, or the claim is lost/stolen
        if (tree == null || !tree.IsClaimed || tree.claimedByAgent != agentObj)
        {
            bb.ui?.SetState("Lost tree target/claim");
            ClearAgentTarget();
            bb.currentTarget = null;
            return _state = NodeState.Failure;
        }

        // Not at tree yet
        if (isMoving)
            return _state = NodeState.Running;

        // Inventory full stop chopping
        if (bb.inventory.IsFull(ResourceType.Wood))
        {
            bb.ui?.SetState("Inventory full");
            tree.ReleaseClaim(agentObj); // RELEASE CLAIM
            ClearAgentTarget();
            bb.currentTarget = null;
            timer = 0;
            return _state = NodeState.Success;
        }

        // Chop animation / delay
        bb.ui?.SetState("Chopping...");

        timer += Time.deltaTime;
        if (timer < chopTime)
            return _state = NodeState.Running;

        // Chop completed reset timer
        timer = 0f;

        // Try to harvest wood from the tree
        int harvestAmount = tree.HarvestWood(bb.stats.chopWoodRate);

        // Add to inventory (may accept less)
        int accepted = bb.inventory.AddResource(ResourceType.Wood, harvestAmount);

        // If not all wood was accepted inventory full
        if (accepted < harvestAmount)
        {
            tree.ReleaseClaim(agentObj); // RELEASE CLAIM
            ClearAgentTarget();
            bb.currentTarget = null;
            return _state = NodeState.Success;
        }

        // If tree still has wood AND inventory still has space keep chopping
        if (tree.currentWoodCapacity > 0 && !bb.inventory.IsFull(ResourceType.Wood))
        {
            return _state = NodeState.Running;
        }

        // Tree empty OR inventory full finished chopping
        tree.ReleaseClaim(agentObj); // RELEASE CLAIM
        bb.currentTarget = null;
        ClearAgentTarget();
        return _state = NodeState.Success;
    }

    private void ClearAgentTarget()
    {
        if (bb.mover != null) bb.mover.ClearTarget();
        if (bb.mlBrain != null) bb.mlBrain.ClearTarget();
    }
}