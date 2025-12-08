using UnityEngine;

public class AgentStatsManager : MonoBehaviour
{
    [Header("Core Stats")]
    public AgentStat health = new AgentStat { currentValue = 100, maxValue = 100 };
    public AgentStat stamina = new AgentStat { currentValue = 100, maxValue = 100, regenRate = 2f, decayRate = 0f };
    public AgentStat hunger = new AgentStat { currentValue = 0, maxValue = 100, decayRate = 1f }; // hunger increases over time
    public AgentStat thirst = new AgentStat { currentValue = 0, maxValue = 100, decayRate = 1.5f }; // thirst increases over time

    [Header("Other Stats")]
    public int drinkRate = 20;
    public int chopWoodRate = 20;
    public int foodHarvestRate = 1;


    [Header("Modifiers")]
    public float starvationDamageRate = 2f; // HP lost per second when hunger is full
    public float dehydrationDamageRate = 3f; // HP lost per second when thirst is full
    public float exhaustionPenalty = 0.5f; // stamina influences speed later

    public bool IsDead => health.currentValue <= 0f;

    void Start()
    {
        // Randomize stats slightly on spawn
        health.currentValue = Random.Range(60f, 100f);
        stamina.currentValue = Random.Range(100f, 100f);
        hunger.currentValue = Random.Range(60, 100f);
        thirst.currentValue = Random.Range(60, 100f);
    }


    void Update()
    {
        // 1. Decrease hunger/thirst over time
        hunger.Decrease(hunger.decayRate * Time.deltaTime);
        thirst.Decrease(thirst.decayRate * Time.deltaTime);

        // 2. Decrease stamina slightly if hungry
        //if (hunger.GetPercent() < 0.2f)
        //    stamina.Decrease(Time.deltaTime * 0.5f);
        //if (thirst.GetPercent() < 0.2f)
        //    stamina.Decrease(Time.deltaTime * 0.5f);



        //// 3. Regenerate stamina when idle or resting (you’ll hook this later)
        //if (stamina.regenRate > 0)
        //    stamina.Increase(stamina.regenRate * Time.deltaTime);

        // 4. If hunger/thirst none, health decays
        if (hunger.GetPercent() <= 0f)
            health.Decrease(starvationDamageRate * Time.deltaTime);
        if (thirst.GetPercent() <= 0f)
            health.Decrease(dehydrationDamageRate * Time.deltaTime);

        // Clamp and check death
        if (health.IsEmpty())
        {
            health.currentValue = 0;
            OnDeath();
        }
    }

    private void OnDeath()
    {
        // TODO: Replace with your agent death logic later
        Debug.Log($"{name} has died of starvation.");
        Destroy(gameObject, 0.2f);
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
