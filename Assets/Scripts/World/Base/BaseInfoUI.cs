// File: BaseInfoUI.cs (REVISED IMPLEMENTATION)

using TMPro;
using UnityEngine;
using UnityEngine.UI; // Included just in case for a full Unity context

public class BaseInfoUI : MonoBehaviour
{
    [Header("UI References")]
    // Renamed 'Panel' to 'panelRoot' for clearer reference to the main container
    public GameObject panelRoot;
    public TMP_Text foodResourceText;
    public TMP_Text waterResourceText;
    public TMP_Text woodResourceText;

    private FireBase currentBase;

    void Start()
    {
        // Initialize by hiding the panel
        HideInfo();
    }

    void Update()
    {
        // Continuously update the UI while the base is selected
        if (currentBase != null)
        {
            UpdateResourceText();
        }
    }

    /// <summary>
    /// Activates the UI and sets the target FireBase to display.
    /// </summary>
    public void ShowInfo(FireBase baseRef)
    {
        currentBase = baseRef;
        if (panelRoot != null) panelRoot.SetActive(true);
        UpdateResourceText();
    }

    /// <summary>
    /// Hides the UI panel.
    /// </summary>
    public void HideInfo()
    {
        currentBase = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void UpdateResourceText()
    {
        if (currentBase == null) return;

        // Directly access the serializable FireBaseStorage data
        FireBaseStorage storage = currentBase.storage;

        // Update the text fields with current/max values
        foodResourceText.text = $"<color=#00CC00>Food:</color> {storage.currentFoodAmount} / {storage.maxFoodAmount}";
        woodResourceText.text = $"<color=#CC6600>Wood:</color> {storage.currentWoodAmount} / {storage.maxWoodAmount}";
        waterResourceText.text = $"<color=#0066CC>Water:</color> {storage.currentWaterAmount} / {storage.maxWaterAmount}";
    }
}