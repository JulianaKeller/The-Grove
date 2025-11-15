using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimalSpecies", menuName = "Species/AnimalSpecies")]
public class AnimalSpeciesData : EntitySpeciesData
{
    public int baseDominance; //indicates spot in the foodchain
    public int dominanceVariation;
    public float walkingSpeed;
    public float runningSpeed;
    public float stamina; //for how long an animal can run
    public float power; //attack strength
    public float defense; //how much damage from an attack/injury is dampened, 1 = full damage deflection
    public float maxHP; //ToDo add some randomness
    public float aggression; //0-1, how likely the animal is to retaliate against attacks vs running away
    public int awarenessRange; //In cells
    public float stealth; //how likely the animal is to not be spotted
    public float energyDepletionRate;
    public float hpRecoveryRate;
    public float hungerRate;
    public float foodNeed; //1-100
    public float thirstRate;
    public float waterNeed; //1-100
    public bool isCarnivore;
    public bool isHerbivore;

    // List of entity types this species can consume
    public PlantSpeciesData[] ediblePlants;
    public AnimalSpeciesData[] edibleAnimals;
    [HideInInspector] public HashSet<AnimalSpeciesData> fearedAnimals; // auto-filled at startup

    public static bool operator ==(AnimalSpeciesData a, AnimalSpeciesData b)
    {
        if (a is null || b is null) return false;

        return a.speciesName == b.speciesName;
    }

    public static bool operator !=(AnimalSpeciesData a, AnimalSpeciesData b)
    {
        return !(a == b);
    }
}
