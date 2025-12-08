using UnityEngine;
using UnityEngine.EventSystems;

public class AgentSelectionManager : MonoBehaviour
{
    public static AgentSelectionManager Instance;

    [Header("Current Selection")]
    public AgentStatsManager selectedAgent;

    [Header("UI Reference")]
    public AgentInfoUI infoUI;
    public BaseInfoUI baseUI;

    private Camera mainCamera;

    // Track hover target
    private I_Interactable lastHovered;

    public static event System.Action<I_Interactable> OnInteractableClicked;
    public static event System.Action<AgentStatsManager> OnAgentSelected;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        HandleHover();

        // Left-click — select or interact directly
        if (Input.GetMouseButtonDown(0))
            HandleLeftClick();

        // Right-click — command selected agent to interact
        if (Input.GetMouseButtonDown(1))
            HandleRightClick();
    }

    // Handle hover feedback for interactable objects
    private void HandleHover()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        I_Interactable hovered = hit.collider != null ? hit.collider.GetComponent<I_Interactable>() : null;

        if (hovered != lastHovered)
        {
            lastHovered?.OnHoverExit();
            if (hovered != null)
                hovered.OnHoverEnter();

            lastHovered = hovered;
        }
    }

    // Handle left-click — select agents or interact directly
    private void HandleLeftClick()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            // Interactable object
            I_Interactable interactable = hit.collider.GetComponent<I_Interactable>();
            if (interactable != null)
            {
                FireBase fb = hit.collider.GetComponent<FireBase>();
                if (fb != null)
                {
                    baseUI?.ShowInfo(fb); // Show the base info UI
                    Debug.Log("Base clicked");
                    // Prevent AgentInfoUI from popping up if the base is clicked
                }
                interactable.Interact(selectedAgent != null ? selectedAgent.gameObject : null);
                OnInteractableClicked?.Invoke(interactable);
                return;
            }

            // Agent
            AgentStatsManager agent = hit.collider.GetComponent<AgentStatsManager>();
            if (agent != null)
            {
                SelectAgent(agent);
                OnAgentSelected?.Invoke(agent);
                return;
            }
        }

        // Empty space
        DeselectAgent();
    }

    // Handle right-click — command selected agent to interact with something
    private void HandleRightClick()
    {
        if (selectedAgent == null) return;

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (!hit) return;

        I_Interactable interactable = hit.collider.GetComponent<I_Interactable>();
        if (interactable != null)
        {
            Debug.Log($"Commanding {selectedAgent.name} to interact with {hit.collider.name}");
            interactable.Interact(selectedAgent.gameObject);
        }
    }

    // Agent Selection
    public void SelectAgent(AgentStatsManager agent)
    {
        if (selectedAgent == agent) return;
        selectedAgent = agent;
        Debug.Log($"Selected agent: {agent.name}");
        infoUI?.ShowInfo(agent);
    }

    // Deselect agent
    public void DeselectAgent()
    {
        if (selectedAgent == null) return;
        selectedAgent = null;
        infoUI?.HideInfo();
    }
}
