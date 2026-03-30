using UnityEngine;

public class RevealAndMemorizeNode : Node
{
    private AgentBlackBoard bb;
    private int revealRadius = 3; // How far the explorer can see

    public RevealAndMemorizeNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        // 1. Reveal the area around the explorer
        bb.mover.grid.RevealArea(bb.mover.transform.position, revealRadius);

        // 2. Look around for resources in this newly revealed area
        Collider2D[] hits = Physics2D.OverlapCircleAll(bb.mover.transform.position, revealRadius * 1.5f);

        foreach (Collider2D hit in hits)
        {
            Food f = hit.GetComponent<Food>();
            if (f != null && !bb.localFoundFoods.Contains(f)) bb.localFoundFoods.Add(f);

            Tree t = hit.GetComponent<Tree>();
            if (t != null && !bb.localFoundTrees.Contains(t)) bb.localFoundTrees.Add(t);

            WaterSource w = hit.GetComponent<WaterSource>();
            if (w != null && !bb.localFoundWaters.Contains(w)) bb.localFoundWaters.Add(w);
        }

        // 3. Mark that we explored a spot
        bb.fogTilesRevealed++;
        bb.ui?.SetState($"Explored area! ({bb.fogTilesRevealed}/{bb.maxExplorationsBeforeReturn})");

        return NodeState.Success;
    }
}