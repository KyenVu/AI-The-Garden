using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsMissingWoodNode : Node
{
    private FireBase fireBase;
    public IsMissingWoodNode(FireBase fireBase)
    {
        this.fireBase = fireBase;
    }

    public override NodeState Evaluate()
    {
        if (fireBase == null)
        {
            // If no base exists, treat as "missing wood" or error
            return _state = NodeState.Failure;
        }
        Debug.Log("Checking if fire base is missing wood...");
        return _state = fireBase.HasSpaceFor(ResourceType.Wood)
            ? NodeState.Success
            : NodeState.Failure;
    }

}
