using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgentInfoUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public TMP_Text agentNameText;
    public Slider healthBar;
    public Slider staminaBar;
    public Slider hungerBar;
    public Slider thirstyBar;

    private AgentStatsManager currentAgent;

    void Start()
    {
        HideInfo();
    }

    void Update()
    {
        if (currentAgent == null) return;

        // Update UI live
        healthBar.value = currentAgent.health.GetPercent();
        staminaBar.value = currentAgent.stamina.GetPercent();
        hungerBar.value = currentAgent.hunger.GetPercent();
        thirstyBar.value = currentAgent.thirst.GetPercent();
    }

    public void ShowInfo(AgentStatsManager agent)
    {
        currentAgent = agent;
        agentNameText.text = agent.name;
        panelRoot.SetActive(true);
    }

    public void HideInfo()
    {
        currentAgent = null;
        panelRoot.SetActive(false);
    }
}
