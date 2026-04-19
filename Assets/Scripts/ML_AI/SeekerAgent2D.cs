using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class SeekerAgent2D : Agent
{
    public Transform target;
    public Transform floor; // <--- Drag your grey floor square in here!
    private Rigidbody2D rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 1. THE SETUP
    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector2.zero;

        // Added Mathf.Max so if the floor is tiny, the range doesn't break into negative numbers
        float rangeX = Mathf.Max(0, (floor.localScale.x / 2f) - 0.5f);
        float rangeY = Mathf.Max(0, (floor.localScale.y / 2f) - 0.5f);

        transform.localPosition = new Vector3(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY), 0);
        target.localPosition = new Vector3(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY), 0);

        // THE ESCAPE HATCH
        int attempts = 0;

        while (Vector3.Distance(transform.localPosition, target.localPosition) < 1.5f && attempts < 50)
        {
            target.localPosition = new Vector3(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY), 0);
            attempts++; // Add 1 to attempts every loop
        }
    }

    // 2. THE SENSES
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)transform.localPosition);
        sensor.AddObservation((Vector2)target.localPosition);
        sensor.AddObservation(rb.velocity);
    }

    // 3. THE MUSCLES & REWARDS
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        rb.AddForce(new Vector2(moveX, moveY) * 10f);

        float distance = Vector2.Distance(transform.localPosition, target.localPosition);

        // REWARD: Did we touch the target?
        if (distance < 1.0f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // PUNISHMENT: Dynamically check if we fell off the floor!
        // If the agent's local position is larger than half the floor's size, they fell off.
        float outOfBoundsX = (floor.localScale.x / 2f) + 0.5f;
        float outOfBoundsY = (floor.localScale.y / 2f) + 0.5f;

        if (Mathf.Abs(transform.localPosition.x) > outOfBoundsX ||
            Mathf.Abs(transform.localPosition.y) > outOfBoundsY)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    // 4. THE MANUAL OVERRIDE
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
        continuousActionsOut[1] = Input.GetAxisRaw("Vertical");
    }
}