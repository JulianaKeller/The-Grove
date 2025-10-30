using UnityEngine;

[CreateAssetMenu(menuName = "Species/PlantSpecies")]
public class PlantSpeciesData : ScriptableObject
{
    public string speciesName;
    public float growthRate;
    public float maxAge;
    public float spreadChance;
}
