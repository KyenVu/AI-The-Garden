// File: Scripts/BehaviorTree/Action/ChopTreeNode.cs (modified)

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

        // FAIL if target is invalid, is not a tree, or the claim is lost/stolen
        if (tree == null || !tree.IsClaimed || tree.claimedByAgent != bb.mover.gameObject)
        {
            bb.ui?.SetState("Lost tree target/claim");
            bb.mover.ClearTarget();
            bb.currentTarget = null;
            return _state = NodeState.Failure;
        }


        // Not at tree yet
        if (!bb.mover.HasReachedDestination())
            return _state = NodeState.Running;

        //  Inventory full stop chopping
        if (bb.inventory.IsFull(ResourceType.Wood))
        {
            Debug.Log("Inventory full! Stop chopping.");
            bb.ui?.SetState("Inventory full");

            tree.ReleaseClaim(bb.mover.gameObject); // RELEASE CLAIM

            bb.mover.ClearTarget();
            bb.currentTarget = null;
            timer = 0;
            return _state = NodeState.Success;
        }

        // Chop animation / delay
        Debug.Log("Chopping tree...");
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
        Debug.Log(bb.inventory.GetAmount(ResourceType.Wood));

        // If not all wood was accepted inventory full
        if (accepted < harvestAmount)
        {
            Debug.Log("Inventory became full mid-chop!");

            tree.ReleaseClaim(bb.mover.gameObject); // RELEASE CLAIM

            bb.mover.ClearTarget();
            bb.currentTarget = null;
            return _state = NodeState.Success;
        }

        // If tree still has wood AND inventory still has space keep chopping
        if (tree.currentWoodCapacity > 0 && !bb.inventory.IsFull(ResourceType.Wood))
        {
            return _state = NodeState.Running;
        }

        // Tree empty OR inventory full finished chopping
        tree.ReleaseClaim(bb.mover.gameObject); // RELEASE CLAIM

        bb.currentTarget = null;
        bb.mover.ClearTarget();
        return _state = NodeState.Success;
    }
}