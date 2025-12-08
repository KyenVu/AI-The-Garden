using UnityEngine;

public class FindTreeNode : Node
{
    private AgentBlackBoard bb;
    public FindTreeNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        Collider2D[] detectedTrees = Physics2D.OverlapCircleAll(bb.mover.transform.position, bb.treeSearchRadius, bb.treeLayer);
        if (detectedTrees.Length == 0)
        {
            bb.ui?.SetState("No Trees In Range");
            _state = NodeState.Failure;
            return _state;
        }

        Transform closestTree = null;
        float minDistance = Mathf.Infinity;

        // Find the closest tree that is not claimed by another agent
        foreach (Collider2D treeCollider in detectedTrees)
        {
            Tree treeComponent = treeCollider.GetComponent<Tree>();

            // Only consider trees that are unclaimed OR already claimed by THIS agent (for task resumption)
            if (treeComponent != null && (treeComponent.IsClaimed == false || treeComponent.claimedByAgent == bb.mover.gameObject))
            {
                float dist = Vector2.Distance(bb.mover.transform.position, treeCollider.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTree = treeCollider.transform;
                }
            }
        }

        // If a valid (unclaimed/self-claimed) tree was found
        if (closestTree != null)
        {
            Tree treeComponent = closestTree.GetComponent<Tree>();

            // Try to claim the resource before setting destination
            if (treeComponent != null && treeComponent.TryClaim(bb.mover.gameObject))
            {
                bb.currentTarget = closestTree;
                bb.mover.SetTarget(closestTree);
                bb.ui?.SetState($"Tree Found - Moving to {closestTree.name}");
                _state = NodeState.Success;
                return _state;
            }
        }

        // If the loop finished without finding an available tree
        bb.ui?.SetState("All nearby trees claimed");
        _state = NodeState.Failure;
        return _state;
    }
}