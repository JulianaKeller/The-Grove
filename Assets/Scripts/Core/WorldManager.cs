using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EnvironmentGrid;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(ResourceManager))]
[RequireComponent(typeof(PlantManager))]
[RequireComponent(typeof(AnimalManager))]
[RequireComponent(typeof(WaterSourceManager))]
public class WorldManager : MonoBehaviour
{
    //This script manages time steps and updates all systems
    public static WorldManager Instance { get; private set; }

    [UnityEngine.Range(0f, 100f)]
    public float balance;
    [UnityEngine.Range(0f, 100f)]
    public float diversity;

    public GroundFertilityTexture groundFertilityTexture;

    public float timeStep = 0.1f;

    private float accumulator = 0f;
    private int tickCount = 0;

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

    void Update()
    {
        accumulator += Time.deltaTime;
        while (accumulator >= timeStep)
        {
            Tick();
            accumulator -= timeStep;
            tickCount++;
        }
    }

    private void Tick()
    {
        //Debug.Log("Tick");
        ResourceManager.Instance.UpdateTokens(timeStep);
        EnvironmentGrid.Instance.UpdateGrid(timeStep);
        PlantManager.Instance.UpdatePlants(timeStep, tickCount);
        AnimalManager.Instance.UpdateAnimals(timeStep, tickCount);
        WaterSourceManager.Instance.UpdateWaterSources(timeStep, tickCount);
        //EventManager.Instance.UpdateEvents(timeStep);

        if (tickCount % 10 == 0 && groundFertilityTexture != null && groundFertilityTexture.enabled)
        {
            EnvironmentGrid.Instance.UpdateGrid(timeStep);
            groundFertilityTexture.UpdateFertilityTexture();
        }

        if (tickCount % 10 == 0)
            EcosystemMetrics.Instance.UpdateBalance();
    }

    public void SpawnStartingSpecies<T>(T[] startingSpecies, Action<T, Vector3> spawnAction, int amount)
    {
        if (startingSpecies != null && startingSpecies.Length > 0)
        {
            for (int i = 0; i < amount; i++)
            {
                for (int k = 0; k < startingSpecies.Length; k++)
                {
                    float gridCenterX = EnvironmentGrid.Instance.gridCenter.x;
                    float gridCenterZ = EnvironmentGrid.Instance.gridCenter.z;
                    float gridSpan = EnvironmentGrid.Instance.gridSize * EnvironmentGrid.Instance.cellSize * 0.5f;
                    Vector3 SpawnPosition = new Vector3(UnityEngine.Random.Range(gridCenterX - gridSpan, gridCenterX + gridSpan), 0f, UnityEngine.Random.Range(gridCenterZ - gridSpan, gridCenterZ + gridSpan));
                    spawnAction(startingSpecies[k], SpawnPosition);
                }
            }
        }
    }

    private List<Entity> GetNearbyEntities(Vector3 pos, int range, bool getAnimals, bool getPlants)
    {
        List<Entity> nearby = new List<Entity>();

        foreach (GridCell cell in GetNearbyCells(pos, range))
        {
            if (getAnimals)
            {
                nearby.AddRange(cell.animals);
            }
            if (getPlants)
            {
                nearby.AddRange(cell.plants);
            }
        }
        return nearby;
    }

    private List<GridCell> GetNearbyCells(Vector3 pos, int range)
    {
        List<GridCell> cells = new List<GridCell>();

        Vector2Int center = EnvironmentGrid.Instance.GetCellCoords(pos);

        cells.Add(EnvironmentGrid.Instance.grid[center.x, center.y]);

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                int gx = center.x + x;
                int gz = center.y + y;

                if (gx < 0 || gz < 0 || gx >= EnvironmentGrid.Instance.gridSize || gz >= EnvironmentGrid.Instance.gridSize)
                    continue;

                var cell = EnvironmentGrid.Instance.grid[gx, gz];
                
                cells.Add(cell);
            }
        }
        return cells;
    }

    public List<Entity> GetNearbyEntities(Vector3 pos, int range)
    {
        return GetNearbyEntities(pos, range, true, true);
    }

    public List<WaterSource> GetNearbyWaterSources(Vector3 pos, int range)
    {
        List<WaterSource> nearby = new List<WaterSource>();

        foreach (GridCell cell in GetNearbyCells(pos, range))
        {
            nearby.AddRange(cell.waterSources);
        }
        return nearby.Distinct().ToList();
    }

    public List<Animal> GetNearbyAnimals(Vector3 pos, int range)
    {
        return GetNearbyEntities(pos, range, true, false).Cast<Animal>().ToList();
    }

    public List<Plant> GetNearbyPlants(Vector3 pos, int range)
    {
        return GetNearbyEntities(pos, range, false, true).Cast<Plant>().ToList();
    }
}
