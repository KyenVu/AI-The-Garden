using UnityEngine;

public class IsMissingResourceInformationNode : Node
{
    private AgentBlackBoard bb;

    public IsMissingResourceInformationNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        // Clean null references first
        bb.knownTrees.RemoveAll(t => t == null);
        bb.knownFoods.RemoveAll(f => f == null);
        bb.knownWaters.RemoveAll(w => w == null);

        // If the worker knows nothing about the world, it needs to go to base
        if (bb.knownTrees.Count == 0 && bb.knownFoods.Count == 0 && bb.knownWaters.Count == 0)
        {
            bb.ui?.SetState("No resources known. Heading to base for info...");
            return NodeState.Success;
        }

        return NodeState.Failure;
    }
}