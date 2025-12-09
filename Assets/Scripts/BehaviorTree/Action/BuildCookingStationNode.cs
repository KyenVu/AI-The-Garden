// File: Scripts/BehaviorTree/Action/BuildCookingStationNode.cs (FINAL MODIFIED VERSION)

using UnityEngine;

public class BuildCookingStationNode : Node
{
    private AgentBlackBoard bb;
    private const int BUILD_COST_FOOD = 10;
    private float buildTime = 3f;
    private float timer = 0f;

    public BuildCookingStationNode(AgentBlackBoard blackBoard)
    {
        this.bb = blackBoard;
    }

    public override NodeState Evaluate()
    {
        // 1. Ensure the agent is at the chosen build location (near base)
        if (!bb.mover.HasReachedDestination())
        {
            bb.ui?.SetState("Moving to build site...");
            return _state = NodeState.Running;
        }

        // 2. Simulate build time
        bb.ui?.SetState($"Building Cooking Station... ({timer:0.0}/{buildTime:0.0})");
        timer += Time.deltaTime;

        if (timer < buildTime)
        {
            return _state = NodeState.Running;
        }

        // 3. Check for race condition
        if (CookingStation.Instance != null)
        {
            timer = 0;
            bb.ui?.SetState("Station already built.");
            return _state = NodeState.Success;
        }

        // 4. Perform final actions: Deduct Cost and Instantiate
        int removedFood = bb.baseRef.RemoveResource(ResourceType.Food, BUILD_COST_FOOD);

        if (removedFood != BUILD_COST_FOOD)
        {
            bb.ui?.SetState("Build failed: Insufficient Food.");
            timer = 0;
            return _state = NodeState.Failure;
        }

        Vector3 buildPos = bb.mover.transform.position;

        // Instantiate the Cooking Station
        GameObject newStationGO = GameObject.Instantiate(
            bb.cookingStationPrefab,
            buildPos,
            Quaternion.identity
        );

        // --- NEW: SET SORTING LAYER AND ORDER ---
        SpriteRenderer sr = newStationGO.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Second Tile Layer";

        }
        // ----------------------------------------

        newStationGO.name = "Cooking Station";
        bb.ui?.SetState("Cooking Station Built!");
        bb.mover.ClearTarget();
        timer = 0;
        return _state = NodeState.Success;
    }
}