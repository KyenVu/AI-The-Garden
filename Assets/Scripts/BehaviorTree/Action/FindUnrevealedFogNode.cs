using UnityEngine;

public class FindUnrevealedFogNode : Node
{
    private AgentBlackBoard bb;

    public FindUnrevealedFogNode(AgentBlackBoard blackBoard) { this.bb = blackBoard; }

    public override NodeState Evaluate()
    {
        // Randomly search for a tile that is NOT revealed
        for (int i = 0; i < 50; i++)
        {
            TileData rndTile = bb.mover.grid.GetRandomWalkableTile();
            if (rndTile != null && !rndTile.isRevealed)
            {
                // Found a fog tile! Set it as the target.
                Vector3 targetPos = new Vector3(rndTile.x, rndTile.y, 0);

                // We create a temporary empty GameObject to act as a Transform target for the Mover
                GameObject tempTarget = new GameObject("FogTarget");
                tempTarget.transform.position = targetPos;
                // Destroy it after 10 seconds to prevent clutter
                GameObject.Destroy(tempTarget, 10f);

                bb.currentTarget = tempTarget.transform;
                bb.mover.SetTarget(bb.currentTarget);

                bb.ui?.SetState("Moving to explore fog...");
                return NodeState.Success;
            }
        }

        bb.ui?.SetState("No more fog to explore!");
        return NodeState.Failure;
    }
}