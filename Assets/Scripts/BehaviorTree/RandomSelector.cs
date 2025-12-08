// File: Scripts/BehaviorTree/RandomSelector.cs (MODIFIED - Replaces your previous RandomSelector)

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Selector that randomizes priority, but ensures that a currently 
/// RUNNING child is prioritized over new random choices.
/// </summary>
public class RandomSelector : Node
{
    private List<Node> children;
    // Track the currently running child instance for persistence
    private Node runningChild = null;

    public RandomSelector(List<Node> nodes)
    {
        this.children = nodes;
    }

    public override NodeState Evaluate()
    {
        // 1. CHECK RUNNING CHILD FIRST (PERSISTENCE)
        if (runningChild != null)
        {
            NodeState result = runningChild.Evaluate();

            if (result == NodeState.Running)
            {
                _state = NodeState.Running;
                return _state; // Agent stays locked on this task
            }

            // If the running child finished (Success or Failure), clear its reference
            runningChild = null;

            // If it Succeeded, the selector succeeds immediately.
            if (result == NodeState.Success)
            {
                _state = NodeState.Success;
                return _state;
            }

            // If it Failed, proceed to random selection for a new task.
        }

        // 2. SHUFFLE THE LIST (RANDOM SELECTION)
        Shuffle(children);

        // 3. EVALUATE CHILDREN IN RANDOM ORDER
        foreach (Node node in children)
        {
            NodeState result = node.Evaluate();

            switch (result)
            {
                case NodeState.Success:
                    _state = NodeState.Success;
                    return _state;

                case NodeState.Running:
                    runningChild = node; // Set the new running child for the next frame
                    _state = NodeState.Running;
                    return _state;

                case NodeState.Failure:
                    // Try next randomly chosen child
                    continue;
            }
        }

        // If all children fail
        _state = NodeState.Failure;
        return _state;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}