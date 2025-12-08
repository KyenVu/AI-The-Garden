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
            stateUI?.SetState("Moving to Food...");
            return _state = NodeState.Running;
        }

        Debug.Log(mover.HasReachedDestination());
        // Simulate eating duration
        stateUI?.SetState("Eating...");
        eatTimer += Time.deltaTime;

        if (eatTimer >= eatDuration)
        {
            Food food = mover.currentTarget.GetComponent<Food>();

            if (food != null && food.data != null)
            {
                // Apply hunger + stamina recovery
                stats.EatFood(food.data.nutritionValue);
                stats.Rest(food.data.staminaRestore);

                food.OnEaten();
                if (food.data.destroyAfterEat)
                    GameObject.Destroy(food.gameObject);
            }
            else
            {
                Debug.LogWarning("Food object missing FoodData!");
            }

            mover.ClearTarget();
            eatTimer = 0f;
            return _state = NodeState.Success;
        }

        return _state = NodeState.Running;
    }
}
