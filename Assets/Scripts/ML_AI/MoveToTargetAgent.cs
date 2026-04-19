using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MoveToTargetAgent : Agent
{
    public Transform targetTransform;
    public float moveSpeed = 1f;
    public Color winColor = Color.green;
    public Color loseColor = Color.red;
    public GameObject floor;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, moveY, 0) * Time.deltaTime * moveSpeed;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(targetTransform.position);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.TryGetComponent<Target>(out Target target))
            {
                SetReward(1f);
                floor.GetComponent<SpriteRenderer>().color = winColor;
                EndEpisode();
            }
            if (collision.TryGetComponent<Wall>(out Wall wall))
            {
                SetReward(-1f);
                floor.GetComponent<SpriteRenderer>().color = loseColor;
                EndEpisode();
            }
        }
       
    }
}