// File: Scripts/BehaviorTree/Action/EatFoodNode.cs (MODIFIED to handle Station)

using UnityEngine;

public class EatFoodNode : Node
{
    private AgentStatsManager stats;
    private AgentMover mover;
    private AgentStateUI stateUI;

    private float eatDuration = 2f;
    private float eatTimer = 0f;

    public EatFoodNode(AgentStatsManager stats, AgentMover mover, AgentStateUI ui)
    {
        this.stats = stats;
        this.mover = mover;
        this.stateUI = ui;
    }

    public override NodeState Evaluate()
    {
        if (mover.currentTarget == null || !mover.HasReachedDestination())
        {
            stateUI?.SetState("Moving to Food Source...");
            return _state = NodeState.Running;
        }

        stateUI?.SetState("Eating...");
        eatTimer += Time.deltaTime;

        if (eatTimer >= eatDuration)
        {
            eatTimer = 0f;

            // --- NEW: Handle consumption from CookingStation ---
            CookingStation station = mover.currentTarget.GetComponent<CookingStation>();
            if (station != null)
            {
                if (station.cookedFoodAvailable > 0)
                {
                    // Use station's gather method (which provides stats/rest bonus)
                    int nutritionValue = station.GatherFood(stats);
                    stats.EatFood(nutritionValue);
                    station.cookedFoodAvailable -= nutritionValue; // Deduct the consumed amount from station's storage

                    station.ReleaseClaim(mover.gameObject);
                    mover.ClearTarget();
                    return _state = NodeState.Success;
                }
                else
                {
                    // If food was taken by another agent, fail the consumption
                    station.ReleaseClaim(mover.gameObject);
                    mover.ClearTarget();
                    return _state = NodeState.Failure;
                }
            }
            // ----------------------------------------------------

            // --- Existing: Handle consumption from World Food ---
            Food food = mover.currentTarget.GetComponent<Food>();
            if (food != null && food.data != null)
            {
                // Apply hunger + stamina recovery
                stats.EatFood(food.data.nutritionValue);
                stats.Rest(food.data.staminaRestore);

                food.OnEaten(); // Handles destruction and claim release for world food
            }
            else
            {
                Debug.LogWarning("Food object missing component or data!");
            }

            mover.ClearTarget();
            return _state = NodeState.Success;
        }

        return _state = NodeState.Running;
    }
}