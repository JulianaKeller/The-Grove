using UnityEngine;

public abstract class EntitySpeciesData : ScriptableObject
{
    public string speciesName;
    public GameObject[] prefabs;

    [Header("Visual Variation")]
    public Color[] colorVariants;

    public float lifespan;
    [Range(0f, 1f)]
    public float lifespanVariation = 0.1f;

    public float nutritionBaseValue;

    public static bool operator ==(EntitySpeciesData a, EntitySpeciesData b)
    {
        if (a is null || b is null) return false;

        return a.speciesName == b.speciesName;
    }

    public static bool operator !=(EntitySpeciesData a, EntitySpeciesData b)
    {
        return !(a == b);
    }
}