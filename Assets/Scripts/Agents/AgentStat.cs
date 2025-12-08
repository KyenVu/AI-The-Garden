using System;
using UnityEngine;

[Serializable]
public class AgentStat
{
    [Range(0, 100)] public float currentValue = 100f;
    public float maxValue = 100f;
    public float decayRate = 1f; // how fast it decreases per second
    public float regenRate = 0f; // if it regenerates

    public void Increase(float amount)
    {
        currentValue = Mathf.Min(currentValue + amount, maxValue);
    }

    public void Decrease(float amount)
    {
        currentValue = Mathf.Max(currentValue - amount, 0);
    }

    public float GetPercent()
    {
        return currentValue / maxValue;
    }

    public bool IsLow(float thresholdPercent)
    {
        return GetPercent() <= thresholdPercent;
    }

    public bool IsEmpty() => currentValue <= 0;
}
