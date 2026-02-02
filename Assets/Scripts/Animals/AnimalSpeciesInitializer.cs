using System.Collections.Generic;
using UnityEngine;

public class AnimalSpeciesInitializer
{
    public static void InitializeFearedAnimals()
    {
        AnimalSpeciesData[] allSpecies = Resources.LoadAll<AnimalSpeciesData>("ScriptableObjects/AnimalSpecies");

        // Clear previous data
        foreach (var s in allSpecies)
            s.fearedAnimals = new HashSet<AnimalSpeciesData>();

        //fill feared animals list with animals who have this species as prey
        foreach (var predator in allSpecies)
        {
            if (predator.edibleEntities == null)
                continue;

            foreach (var entity in predator.edibleEntities)
            {
                if (entity == null)
                    continue;

                if(entity is AnimalSpeciesData prey)
                {
                    // prey fears predator
                    if (!prey.fearedAnimals.Contains(predator))
                        prey.fearedAnimals.Add(predator);
                }
            }
        }

        Debug.Log($"Initialized fear relationships for {allSpecies.Length} animal species.");
    }
}
