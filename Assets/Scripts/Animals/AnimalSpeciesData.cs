using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimalSpecies", menuName = "Species/AnimalSpecies")]
public class AnimalSpeciesData : ScriptableObject
{
    public string speciesName;
    public GameObject prefab;
    public float moveSpeed;
    public float sightRange;
    public float hungerRate;
    public float thirstRate;
    public float lifespan; //ToDo randomness
    public bool isPredator;
}
