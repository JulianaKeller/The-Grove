using UnityEngine;

[CreateAssetMenu(menuName = "Species/PlantSpecies")]
public class PlantSpeciesData : ScriptableObject
{
    public string speciesName;
    public GameObject[] prefabs;
    [Header("Visual Variation")]
    public Color[] colorVariants;
    public float maxHP;
    public float waterNeed; //water usage per time step
    public float waterCapacity; //1-100 how much water can be stored in the plant
    public float groundFertilityUsage;
    public float minGroundFertility; //plant will lose health points if positioned on ground with lower fertility
    //public float growthRate;
    public float nutritionBaseValue; //grows with growthRate
    public float lifespan;
    [Range(0f, 1f)]
    public float lifespanVariation = 0.1f;
    public Vector3 maxSizeVariation;
    public float baseSpreadChance;
    public float spreadingRadius;
    public bool isEdible;
}
