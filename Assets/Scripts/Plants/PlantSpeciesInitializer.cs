using System.Collections.Generic;
using UnityEngine;

public class PlantSpeciesInitializer
{
    public static void InitializePlantValues()
    {
        PlantSpeciesData[] allSpecies = Resources.LoadAll<PlantSpeciesData>("ScriptableObjects/PlantSpecies");

        foreach (var s in allSpecies)
        {
           //s.spreadingRadius = EnvironmentGrid.Instance.cellSize * s.spreadingRadius;

            if (s.maxSizeVariation.x <= 0)
            {
                s.maxSizeVariation.x = 1f;
            }
            if (s.maxSizeVariation.y <= 0)
            {
                s.maxSizeVariation.y = 1f;
            }
        }

        Debug.Log($"Initialized fear relationships for {allSpecies.Length} animal species.");
    }
}
