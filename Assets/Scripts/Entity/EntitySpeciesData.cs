using UnityEngine;

public abstract class EntitySpeciesData : ScriptableObject
{
    public string speciesName;
    public GameObject[] prefabs;

    [Header("Visual Variation")]
    public Color[] colorVariants;

    public float lifespan;
    public float maxHP;
    [Range(0f, 1f)]
    public float lifespanVariation = 0.1f;
    public float growthRate = 1f;
    public Vector2 maxSizeVariation = new Vector2(0.2f, 0.2f);

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

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is not EntitySpeciesData other)
            return false;

        return speciesName == other.speciesName;
    }

    public override int GetHashCode()
    {
        return speciesName != null ? speciesName.GetHashCode() : 0;
    }
}