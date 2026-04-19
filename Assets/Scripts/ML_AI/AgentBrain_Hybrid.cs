using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentBrain_Hybrid : Agent
{
    [Header("Hierarchy Managers")]
    public GridManager gridManager;
    public Pathfinding pathfinding; // Your A* Script
    public AgentStateUI ui;

    [Header("Movement Options")]
    public float moveSpeed = 5f;
    public float reachThreshold = 0.4f; // How close to a waypoint to count as "reached"

    private Rigidbody2D rb;

    // --- MACRO PATHFINDING (The Boss) ---
    private List<TileData> currentPath = new List<TileData>();
    private int waypointIndex = 0;

    // --- MICRO PATHFINDING (The Employee) ---
    private Vector2 currentWaypointPos;
    private float lastDistanceToWaypoint;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();

        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (pathfinding == null) pathfinding = FindObjectOfType<Pathfinding>();
    }

    public override void OnEpisodeBegin()
    {
        if (rb != null) rb.velocity = Vector2.zero;

        // Start somewhere random
        transform.localPosition = new Vector3(Random.Range(1f, 5f), Random.Range(1f, 5f), 0);

        // Tell the Boss to find a new mission!
        AssignNewMacroMission();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // The ML Agent ONLY looks at the next immediate waypoint, not the final goal!
        Vector2 dirToWaypoint = (currentWaypointPos - rb.position).normalized;
        sensor.AddObservation(dirToWaypoint);

        // Momentum
        sensor.AddObservation(rb.velocity.normalized);

        // Space Size is exactly 4
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        // 1. Apply Physics Movement
        Vector2 force = new Vector2(moveX, moveY) * moveSpeed;
        rb.velocity = force;

        // 2. Boundary Clamp
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
        AddReward(-0.001f); // Existential Penalty (Hurry up!)

        // 3. Waypoint Logic
        float currentDist = Vector2.Distance(rb.position, currentWaypointPos);

        if (currentDist <= reachThreshold)
        {
            AdvanceToNextWaypoint();
        }
        else
        {
            // Soft Guidance: Reward for getting closer to the immediate waypoint
            if (currentDist < lastDistanceToWaypoint) AddReward(0.002f);
            else if (currentDist > lastDistanceToWaypoint) AddReward(-0.002f);
        }

        lastDistanceToWaypoint = currentDist;

        if (ui != null) ui.SetState("ML: Hybrid A* Pathing");
    }

    // --- HIERARCHY LOGIC ---

    private void AssignNewMacroMission()
    {
        if (gridManager == null || pathfinding == null) return;

        // 1. Pick a random final destination
        TileData safeTarget = gridManager.GetRandomWalkableTile();
        if (safeTarget == null) return;

        Vector3 finalGoalPos = new Vector3(safeTarget.x, safeTarget.y, 0);

        // 2. Ask A* to build the path
        currentPath = pathfinding.FindPath(transform.position, finalGoalPos);

        // 3. If A* fails (no path possible), reset and try a new spot
        if (currentPath == null || currentPath.Count == 0)
        {
            AssignNewMacroMission();
            return;
        }

        // 4. Hand the first waypoint to the Employee
        waypointIndex = 0;
        SetEmployeeWaypoint();
    }

    private void AdvanceToNextWaypoint()
    {
        waypointIndex++;

        // Have we reached the absolute final destination?
        if (waypointIndex >= currentPath.Count)
        {
            AddReward(2.0f); // MASSIVE reward for finishing the whole maze
            AssignNewMacroMission(); // Start a new maze
        }
        else
        {
            // Just reached a middle-waypoint. 
            // Give a medium reward so the AI learns to "eat the breadcrumbs"
            AddReward(0.5f);
            SetEmployeeWaypoint();
        }
    }

    private void SetEmployeeWaypoint()
    {
        TileData nextTile = currentPath[waypointIndex];
        currentWaypointPos = new Vector2(nextTile.x, nextTile.y);
        lastDistanceToWaypoint = Vector2.Distance(rb.position, currentWaypointPos);
    }

    // --- PENALTIES ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Still penalize hitting walls so it learns to step cleanly on the A* tiles
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

    // --- VISUALIZATION ---
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && currentPath != null)
        {
            // Draw the A* Path in Blue
            Gizmos.color = Color.blue;
            foreach (TileData tile in currentPath)
            {
                Gizmos.DrawCube(new Vector3(tile.x, tile.y, 0), Vector3.one * 0.3f);
            }

            // Draw the Employee's Current Target in Green
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(currentWaypointPos.x, currentWaypointPos.y, 0), reachThreshold);
            Gizmos.DrawLine(transform.position, new Vector3(currentWaypointPos.x, currentWaypointPos.y, 0));
        }
    }
}