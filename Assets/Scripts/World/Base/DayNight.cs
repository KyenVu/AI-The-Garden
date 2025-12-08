using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNight : MonoBehaviour
{
    [Header("References")]
    public Light2D globalLight;

    [Header("Time Settings")]
    public float dayLengthInSeconds = 120f; // full cycle = 2 mins
    [Range(0f, 1f)] public float timeOfDay = 0.5f; // 0 = sunrise, 0.5 = sunset, 1 = next sunrise

    [Header("Lighting")]
    public float dayIntensity = 1f;
    public float nightIntensity = 0.1f;
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.1f, 0.1f, 0.35f);

    [Header("Debug")]
    public bool autoRun = true;

    public bool IsNight => timeOfDay < 0.25f;
    public bool IsDay => timeOfDay >= 0.25f;

    private void Update()
    {
        if (autoRun)
            AdvanceTime();

        UpdateLighting();
    }

    void AdvanceTime()
    {
        timeOfDay += Time.deltaTime / dayLengthInSeconds;

        // Loop
        if (timeOfDay >= 1f)
            timeOfDay = 0f;
    }

    void UpdateLighting()
    {
        if (globalLight == null) return;

        // 0.0  0.25 : Sunrise transition  
        // 0.25  0.75 : Bright day  
        // 0.75  1.0 : Sunset  Night

        float intensity;
        Color color;

        if (timeOfDay < 0.25f) // Sunrise
        {
            float t = timeOfDay / 0.25f;
            intensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
            color = Color.Lerp(nightColor, dayColor, t);
        }
        else if (timeOfDay < 0.75f) // Day
        {
            intensity = dayIntensity;
            color = dayColor;
        }
        else // Sunset  Night
        {
            float t = (timeOfDay - 0.75f) / 0.25f;
            intensity = Mathf.Lerp(dayIntensity, nightIntensity, t);
            color = Color.Lerp(dayColor, nightColor, t);
        }

        globalLight.intensity = intensity;
        globalLight.color = color;
    }

    // Helper to set time externally, e.g., SetMorning(); SetNight(); commands
    public void SetTime(float t)
    {
        timeOfDay = Mathf.Clamp01(t);
    }

    public void SetMorning() => SetTime(0f);
    public void SetNoon() => SetTime(0.25f);
    public void SetEvening() => SetTime(0.75f);
    public void SetNight() => SetTime(0.9f);
}
