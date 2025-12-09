// File: Scripts/UI/CookingStationInfoUI.cs (New File)

using TMPro;
using UnityEngine;

public class CookingStationInfoUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public TMP_Text stationLevelText;
    public TMP_Text foodStatusText;
    public TMP_Text cookTimeText;

    private CookingStation currentStation;

    void Start()
    {
        HideInfo();
    }

    void Update()
    {
        // Continuously update the UI while the station is selected
        if (currentStation != null)
        {
            UpdateUI();
        }
    }

    /// <summary>
    /// Activates the UI and sets the target CookingStation to display.
    /// </summary>
    public void ShowInfo(CookingStation station)
    {
        currentStation = station;
        if (panelRoot != null) panelRoot.SetActive(true);
        UpdateUI();
    }

    public void HideInfo()
    {
        currentStation = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void UpdateUI()
    {
        if (currentStation == null) return;

        // Use the level and max level to show status
        stationLevelText.text = $" Cooking Station - Level: {currentStation.level} / {currentStation.maxLevel}";

        // Safely access the current cook time based on the dictionary
        float time = currentStation.cookTimeByLevel.ContainsKey(currentStation.level) ?
            currentStation.cookTimeByLevel[currentStation.level] :
            currentStation.cookTimeByLevel[1];

        cookTimeText.text = $"Cook Speed: {time:0.1}s per Food";

        // Display Food Status (Current / Max)
        foodStatusText.text =
            $"Food Available: {currentStation.cookedFoodAvailable} / {currentStation.maxFoodAvailable}";
    }
}