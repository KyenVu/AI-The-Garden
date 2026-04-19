using UnityEngine;

public class AgentStatsManager : MonoBehaviour
{
    [Header("Core Stats")]
    public AgentStat health = new AgentStat { currentValue = 100, maxValue = 100 };
    public AgentStat stamina = new AgentStat { currentValue = 100, maxValue = 100, regenRate = 2f, decayRate = 0f };
    public AgentStat hunger = new AgentStat { currentValue = 0, maxValue = 100, decayRate = 1f };
    public AgentStat thirst = new AgentStat { currentValue = 0, maxValue = 100, decayRate = 1.5f };

    [Header("Other Stats")]
    public int drinkRate = 20;
    public int chopWoodRate = 20;
    public int foodHarvestRate = 1;

    [Header("Modifiers")]
    public float starvationDamageRate = 2f;
    public float dehydrationDamageRate = 3f;
    public float exhaustionPenalty = 0.5f;

    public bool IsDead => health.currentValue <= 0f;

    // --- CHANGED: Now a public method so the ML Brain can call it on Episode Begin ---
    public void ResetStats()
    {
        health.currentValue = health.maxValue;
        stamina.currentValue = stamina.maxValue;

        // Randomizing start stats forces the AI to check its meters immediately, 
        // rather than blindly memorizing a routine!
        hunger.currentValue = Random.Range(60f, 100f);
        thirst.currentValue = Random.Range(60f, 100f);
    }

    void Update()
    {
        // 1. Decrease hunger/thirst over time
        hunger.Decrease(hunger.decayRate * Time.deltaTime);
        thirst.Decrease(thirst.decayRate * Time.deltaTime);

        // 2. If hunger/thirst none, health decays
        if (hunger.GetPercent() <= 0f)
            health.Decrease(starvationDamageRate * Time.deltaTime);
        if (thirst.GetPercent() <= 0f)
            health.Decrease(dehydrationDamageRate * Time.deltaTime);

        // 3. Clamp health
        if (health.IsEmpty())
        {
            health.currentValue = 0;
            // We removed the Destroy() call. The ML Brain will handle death now!
        }
    }

    // Helper methods for other systems
    public void EatFood(int nutrition)
    {
        hunger.Increase(nutrition);
    }

    public void DrinkWater(int hydration)
    {
        thirst.Increase(hydration);
    }

    public void Rest(float restAmount)
    {
        stamina.Increase(restAmount);
    }

    public void TakeDamage(float damage)
    {
        health.Decrease(damage);
    }

    public void Heal(float amount)
    {
        health.Increase(amount);
    }
}