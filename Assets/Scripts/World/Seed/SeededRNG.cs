using UnityEngine;

public static class SeededRNG
{
    private static System.Random rng;

    public static void Init(int seed)
    {
        rng = new System.Random(seed);
        Debug.Log($"Seeded RNG initialized with seed: {seed}");
    }

    public static float Range(float min, float max)
    {
        return (float)(rng.NextDouble() * (max - min) + min);
    }

    public static int Range(int min, int max)
    {
        return rng.Next(min, max);
    }

    public static bool Chance(float probability)
    {
        return rng.NextDouble() < probability;
    }
}
