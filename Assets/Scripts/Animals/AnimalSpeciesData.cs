using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimalSpecies", menuName = "Species/AnimalSpecies")]
public class AnimalSpeciesData : ScriptableObject
{
    public string speciesName;
    public GameObject prefab;
    [Header("Visual Variation")]
    public Color[] colorVariants;
    public float moveSpeed;
    public float sightRange;
    public float hungerRate;
    public float thirstRate;
    public float lifespan; //ToDo randomness
    [Range(0f, 1f)]
    public float lifespanVariation = 0.1f;
    public bool isPredator;
}
