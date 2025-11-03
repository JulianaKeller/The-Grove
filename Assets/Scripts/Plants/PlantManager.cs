using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    //Updates all plants (growth, death, reproduction)
    public static PlantManager Instance { get; private set; }

    public int updateSubsetCount;

    private List<Plant> plants = new List<Plant>();

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
        
    }

    void Update()
    {
        
    }

    public void UpdatePlants(float timeStep, int tick)
    {
        foreach (var plant in plants)
        {
            if ((tick + plant.id) % updateSubsetCount == 0)  // slower updates
                plant.UpdatePlant(timeStep);
        }
    }

    public void RemovePlant(Plant p)
    {
        plants.Remove(p);
    }
}
