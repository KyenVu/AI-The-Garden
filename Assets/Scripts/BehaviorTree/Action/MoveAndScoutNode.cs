using UnityEngine;
using System.Collections.Generic;

public class MoveAndScoutNode : Node
{
    private AgentBlackBoard bb;
    private int revealRadius = 2;

    public MoveAndScoutNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        if (bb.currentTarget == null) return NodeState.Failure;

        // ==========================================
        // 1. SCAN THE GROUND UNDERNEATH THE AGENT
        // ==========================================
        TileData currentTile = bb.mover.grid.GetTileAtWorldPosition(bb.mover.transform.position);

        // Did we just step into the fog?
        if (currentTile != null && !currentTile.isRevealed)
        {
            // Reveal the area permanently
            bb.mover.grid.RevealArea(bb.mover.transform.position, revealRadius);

            // Memorize resources seen in the newly cleared area
            Collider2D[] hits = Physics2D.OverlapCircleAll(bb.mover.transform.position, revealRadius * 1.5f);
            foreach (Collider2D hit in hits)
            {
                Food f = hit.GetComponent<Food>();
                if (f != null && !bb.knownFoods.Contains(f)) bb.knownFoods.Add(f);

                Tree t = hit.GetComponent<Tree>();
                if (t != null && !bb.knownTrees.Contains(t)) bb.knownTrees.Add(t);

                WaterSource w = hit.GetComponent<WaterSource>();
                if (w != null && !bb.knownWaters.Contains(w)) bb.knownWaters.Add(w);
            }

            bb.fogTilesRevealed++;
           
            bb.ui?.SetState($"Scouting fog ({bb.fogTilesRevealed}/{bb.maxExplorationsBeforeReturn})");

            // If we hit our reveal limit for this trip, stop and head back
            if (bb.fogTilesRevealed >= bb.maxExplorationsBeforeReturn)
            {
                bb.mover.ClearTarget();
                return NodeState.Success;
            }
            if (!bb.mover.HasReachedDestination())
            {
                return NodeState.Running;
            }
        }

        // ==========================================
        // 2. CONTINUE MOVING
        // ==========================================
        if (!bb.mover.HasReachedDestination())
        {
            return NodeState.Running;
        }

        bb.currentTarget = null;
        return NodeState.Success;
    }
}