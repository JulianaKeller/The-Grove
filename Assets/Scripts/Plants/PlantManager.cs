using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    //Updates all plants (growth, death, reproduction)
    public static PlantManager Instance { get; private set; }
    public bool spawnStartingSpecies = false;
    public int updateSubsetCount;
    public GameObject spawnedPlantsParent;

    public PlantSpeciesData[] startingSpecies;
    public int initialSpawnAmount;

    private List<Plant> plants = new List<Plant>();
    private List<PlantView> views = new List<PlantView>();

    private static int nextId = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (spawnStartingSpecies)
        {
            WorldManager.Instance.SpawnStartingSpecies<PlantSpeciesData>(startingSpecies, SpawnPlant, initialSpawnAmount);
        }

        EnvironmentGrid.Instance.PrintGridPlants();
    }

    public void SpawnPlant(PlantSpeciesData species, Vector3 pos)
    {
        if (species.prefabs != null)
        {
            Plant data = new Plant(species, pos, nextId++);
            GameObject original = species.prefabs[Random.Range(0, species.prefabs.Length)];

            GameObject obj = Instantiate(original, pos, Quaternion.identity, spawnedPlantsParent ? spawnedPlantsParent.transform : null);

            PlantView view = obj.GetComponent<PlantView>();

            data.maxSize = original.transform.localScale;
            float variation = Random.Range(-species.maxSizeVariation.x, species.maxSizeVariation.x);

            data.maxSize = data.maxSize + new Vector3(variation, variation, variation);

            view.data = data;
            data.view = view;

            views.Add(view);
            plants.Add(data);

            // Assign random color
            if (species.colorVariants != null && species.colorVariants.Length > 0)
            {
                Color selectedColor = species.colorVariants[Random.Range(0, species.colorVariants.Length)];
                Renderer rend = view.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    // Use a new material instance so that color changes don't affect shared material
                    rend.material = new Material(rend.material);
                    rend.material.color = selectedColor;
                }
            }

            //-----Add to Grid-----
            EnvironmentGrid.Instance.RegisterPlant(data);

            Debug.Log("Spwaned a " + data.species.name);
        }
    }

    public void UpdatePlants(float timeStep, int tick)
    {
        for (int i = plants.Count - 1; i >= 0; i--)
        {
            Plant plant = plants[i];
            if ((tick + plant.id) % updateSubsetCount == 0)
            {
                plant.view.ResetInterpolation();
                plant.UpdatePlant(timeStep);
            }
                
        }
    }

    public void RemovePlant(Plant p)
    {
        plants.Remove(p);
        if (p.view != null)
        {
            views.Remove(p.view);
            Destroy(p.view.gameObject);
        }
        EnvironmentGrid.Instance.DeregisterPlant(p);
    }
}
