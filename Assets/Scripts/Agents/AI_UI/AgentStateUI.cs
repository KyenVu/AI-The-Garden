using TMPro;
using UnityEngine;

public class AgentStateUI : MonoBehaviour
{
    public TextMeshProUGUI stateText;  // Drag UI Text here
    public Transform agent;            // Which agent to follow?
    public Vector3 offset = new Vector3(0, 1.5f, 0);  // Float above head
    void Update()
    {
        if (agent == null) return;

        // Follow agent position
        transform.position = agent.position + offset;
    }

    public void SetState(string text)
    {
        if (stateText != null)
            stateText.text = text;
    }
}
