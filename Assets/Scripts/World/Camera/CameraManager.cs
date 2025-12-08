using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    [Header("Focus Settings")]
    public float focusZoom = 5f;          // Zoom level when focusing
    public float focusDuration = 0.6f;    // Duration for zoom+move tween
    public Ease focusEase = Ease.InOutSine;

    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging;
    private Tween moveTween;
    private Tween zoomTween;

    private void OnEnable()
    {
        AgentSelectionManager.OnInteractableClicked += FocusOnInteractable;
        AgentSelectionManager.OnAgentSelected += (agent) =>
        {
            if (agent != null)
                FocusOn(agent.transform.position, focusZoom);
        };

    }

    private void OnDisable()
    {
        AgentSelectionManager.OnInteractableClicked -= FocusOnInteractable;
    }




    private void Start()
    {
        cam = GetComponent<Camera>();

        // start camera at base position
        if (gridManager != null)
        {
            Vector3 basePos = gridManager.GetBaseLocation();
            transform.position = new Vector3(basePos.x, basePos.y, transform.position.z);
        }
    }

    private void Update()
    {
        HandleDrag();
        HandleZoom();
        HandleReset();
        //HandleFocusClick();
    }

    // Drag with Left Mouse ===
    private void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = Vector3.Lerp(transform.position, transform.position + difference, Time.deltaTime * moveSpeed);

        }
    }

    // Scroll to Zoom ===
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float targetZoom = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);

            // tween zoom smoothly
            cam.DOOrthoSize(targetZoom, 0.4f)
               .SetEase(focusEase)
               .OnUpdate(ClampToGrid);
        }
    }
    private void FocusOnInteractable(I_Interactable interactable)
    {
        if (interactable is MonoBehaviour mb)
            FocusOn(mb.transform.position, focusZoom);
    }

    // Space resets camera to base ===
    private void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (gridManager != null)
            {
                FocusOn(gridManager.GetBaseLocation(), focusZoom); // zoom out a bit on reset
            }
        }
    }

    //  Focus on interactable or agent with right-click ===

    /// <summary>
    /// Smoothly moves and zooms camera toward target.
    /// </summary>
    public void FocusOn(Vector3 worldPos, float targetZoom)
    {
        if (cam == null) cam = GetComponent<Camera>();

        worldPos.z = transform.position.z;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        // Move camera
        moveTween = transform.DOMove(worldPos, focusDuration).SetEase(focusEase);
        // Zoom camera
        zoomTween = DOTween.To(() => cam.orthographicSize, x => cam.orthographicSize = x, targetZoom, focusDuration).SetEase(focusEase).OnUpdate(ClampToGrid);

    }

    private void ClampToGrid()
    {
        transform.position = ClampPosition(transform.position);
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        if (gridManager == null || gridManager.gridConfig == null)
            return position;

        float gridWidth = gridManager.gridConfig.width;
        float gridHeight = gridManager.gridConfig.height;

        float camHalfHeight = focusZoom;
        float camHalfWidth = camHalfHeight * cam.aspect;

        // Calculate boundaries
        float minX = camHalfWidth;
        float maxX = gridWidth - camHalfWidth;
        float minY = camHalfHeight;
        float maxY = gridHeight - camHalfHeight;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        position.z = transform.position.z; // maintain depth
        return position;
    }
}
