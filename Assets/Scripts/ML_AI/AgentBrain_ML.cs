using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentBrain_ML : Agent
{
    [Header("Body Components")]
    public AgentStateUI ui;
    public GridManager gridManager;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float reachThreshold = 0.5f;

    private Rigidbody2D rb;

    // --- THE API VARIABLES ---
    private Vector2 currentTargetPos;
    private bool hasTarget = false;
    private float lastDistanceToTarget;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
    }

    public override void OnEpisodeBegin()
    {
        if (rb != null) rb.velocity = Vector2.zero;
        hasTarget = false; // Reset target on new episode
    }

    // ==========================================
    // EXTERNAL API (For the Behavior Tree Nodes)
    // ==========================================

    public void SetExternalTarget(Vector2 targetPos)
    {
        currentTargetPos = targetPos;
        hasTarget = true;
        lastDistanceToTarget = Vector2.Distance(rb.position, currentTargetPos);
    }

    public void ClearTarget()
    {
        hasTarget = false;
        if (rb != null) rb.velocity = Vector2.zero;
    }

    public bool HasReachedTarget()
    {
        if (!hasTarget) return false;
        return Vector2.Distance(rb.position, currentTargetPos) <= reachThreshold;
    }

    // ==========================================
    // ML-AGENT CORE
    // ==========================================

    public override void CollectObservations(VectorSensor sensor)
    {
        if (hasTarget)
        {
            // If the BT gave us a target, look at it
            Vector2 dirToTarget = (currentTargetPos - rb.position).normalized;
            sensor.AddObservation(dirToTarget);
        }
        else
        {
            // If idling, look nowhere
            sensor.AddObservation(Vector2.zero);
        }

        // Always observe velocity
        sensor.AddObservation(rb.velocity.normalized);

        // Space Size MUST be 4
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!hasTarget)
        {
            rb.velocity = Vector2.zero;
            return; // Stand still if the Behavior Tree hasn't given us a job!
        }

        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        // Apply Movement
        Vector2 force = new Vector2(moveX, moveY) * moveSpeed;
        rb.velocity = force;

        // --- BOUNDARY CLAMP ---
        if (gridManager != null && gridManager.gridConfig != null)
        {
            float maxX = gridManager.gridConfig.width - 1;
            float maxY = gridManager.gridConfig.height - 1;

            Vector2 clampedPos = rb.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, 0, maxX);
            clampedPos.y = Mathf.Clamp(clampedPos.y, 0, maxY);
            rb.position = clampedPos;
        }

        // --- REWARD SYSTEM ---
        AddReward(-0.001f); // Existential Penalty

        float currentDist = Vector2.Distance(rb.position, currentTargetPos);
        if (currentDist < lastDistanceToTarget) AddReward(0.002f);
        else if (currentDist > lastDistanceToTarget) AddReward(-0.002f);

        lastDistanceToTarget = currentDist;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            AddReward(-0.5f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    // --- DEBUGGING VISUALS ---
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && hasTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(currentTargetPos.x, currentTargetPos.y, 0), reachThreshold);
            Gizmos.DrawLine(transform.position, new Vector3(currentTargetPos.x, currentTargetPos.y, 0));
        }
    }
}