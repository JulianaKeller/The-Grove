using UnityEngine;

[CreateAssetMenu(menuName = "Species/PlantSpecies")]
public class PlantSpeciesData : EntitySpeciesData
{
    public float maxHP;
    public float waterNeed; //water usage per time step
    public float waterCapacity; //1-100 how much water can be stored in the plant
    public float groundFertilityUsage;
    public float minGroundFertility; //plant will lose health points if positioned on ground with lower fertility
    //public float growthRate;
    public float growthRate;
    public Vector2 maxSizeVariation;
    public float baseSpreadChance;
    public float spreadingRadius;
    public bool isEdible;

    public static bool operator ==(PlantSpeciesData a, PlantSpeciesData b)
    {
        if (a is null || b is null) return false;

        return a.speciesName == b.speciesName;
    }

    public static bool operator !=(PlantSpeciesData a, PlantSpeciesData b)
    {
        return !(a == b);
    }
}
