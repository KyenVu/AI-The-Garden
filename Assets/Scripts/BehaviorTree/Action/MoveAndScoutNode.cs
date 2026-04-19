using UnityEngine;

public class MoveAndScoutNode : Node
{
    private AgentBlackBoard bb;

    public MoveAndScoutNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {

        // 1. Give the ML Brain the target if it doesn't have it yet
        if (bb.destinationObject != null && bb.mlBrain != null)
        {
            Vector2 targetPos = new Vector2(bb.destinationObject.position.x, bb.destinationObject.position.y);
            bb.mlBrain.SetExternalTarget(targetPos);
        }

        Transform agentTransform = bb.mlBrain != null ? bb.mlBrain.transform : bb.mover.transform;
        GridManager grid = bb.mlBrain != null ? bb.mlBrain.gridManager : bb.mover.grid;

        // 2. THE CONTINUOUS SCOUTING CHECK
        if (grid != null && agentTransform != null)
        {
            TileData currentTile = grid.GetTileAtWorldPosition(agentTransform.position);

            // Did we just step on fog?
            if (currentTile != null && !currentTile.isRevealed)
            {
                // --- WE HIT FOG! USE GRIDMANAGER TO REVEAL 3x3 ---
                // Radius 1 around the center tile = a 3x3 square
                grid.RevealArea(agentTransform.position, 1);

                bb.fogTilesRevealed++;
                bb.ui?.SetState($"Revealed {bb.fogTilesRevealed}/3 patches!");
            }
            if (bb.fogTilesRevealed >= bb.maxExplorationsBeforeReturn) // Or use bb.maxExplorationsBeforeReturn
            {
                bb.ui?.SetState("Limit Reached! Aborting exploration...");

                // Stop moving to the current distant target
                ClearTargets();

                // Return Success to end the Explore Sequence early
                return NodeState.Success;
            }
        }

        // 3. CHECK IF WE REACHED THE TARGET
        bool hasReached = bb.mlBrain != null ? bb.mlBrain.HasReachedTarget() : bb.mover.HasReachedDestination();

        if (!hasReached)
        {
            // We haven't reached the target. Keep walking!
            return NodeState.Running;
        }

        // 4. WE REACHED THE TARGET
        ClearTargets();
        return NodeState.Success;
    }

    private void ClearTargets()
    {
        bb.currentTarget = null;
        bb.destinationObject = null;
        if (bb.mlBrain != null) bb.mlBrain.ClearTarget();
        if (bb.mover != null) bb.mover.ClearTarget();
    }
}