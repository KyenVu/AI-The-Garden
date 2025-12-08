using UnityEngine;

public class DrinkWaterNode : Node
{
    private AgentStatsManager stats;
    private AgentMover mover;
    private AgentStateUI stateUI;

    private float drinkDuration = 2f;
    private float drinkTimer = 0f;

    public DrinkWaterNode(AgentStatsManager stats, AgentMover mover, AgentStateUI ui)
    {
        this.stats = stats;
        this.mover = mover;
        this.stateUI = ui;
    }

    public override NodeState Evaluate()
    {
        // If still moving toward water
        if (mover.currentTarget == null || !mover.HasReachedDestination())
        {
            stateUI?.SetState("Moving to Water...");
            return _state = NodeState.Running;
        }
        Debug.Log(mover.HasReachedDestination());
        // Begin drinking process
        stateUI?.SetState("Drinking...");
        drinkTimer += Time.deltaTime;

        if (drinkTimer >= drinkDuration)
        {
            WaterSource source = mover.currentTarget.GetComponent<WaterSource>();
            if (source != null)
            {
                int hydrationGained = source.Drink(stats.drinkRate);
                stats.DrinkWater(hydrationGained);
            }

            mover.ClearTarget();
            drinkTimer = 0f;
            return _state = NodeState.Success;
        }

        return _state = NodeState.Running;
    }
}
