using UnityEngine;

[CreateAssetMenu(menuName = "Species/PlantSpecies")]
public class PlantSpeciesData : ScriptableObject
{
    public string speciesName;
    public GameObject prefab;
    [Header("Visual Variation")]
    public Color[] colorVariants;
    public float growthRate;
    public float nutritionValue;
    public float lifespan;
    [Range(0f, 1f)]
    public float lifespanVariation = 0.1f;
    public float spreadChance;
}
