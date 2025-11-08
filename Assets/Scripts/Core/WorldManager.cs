using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class WorldManager : MonoBehaviour
{
    //This script manages time steps and updates all systems
    public static WorldManager Instance { get; private set; }

    public GroundFertilityTexture groundFertilityTexture;

    public float timeStep = 0.1f;
    private float accumulator = 0f;
    private int tickCount = 0;

    // ensures only one instance exists
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
        EnvironmentGrid.Instance.UpdateGrid(timeStep);
        PlantManager.Instance.UpdatePlants(timeStep, tickCount);
        AnimalManager.Instance.UpdateAnimals(timeStep, tickCount);
        //EventManager.Instance.UpdateEvents(timeStep);

        if (tickCount % 10 == 0 && groundFertilityTexture != null)
        {
            EnvironmentGrid.Instance.UpdateGrid(timeStep);
            groundFertilityTexture.UpdateFertilityTexture();
        }
    }

    public void SpawnStartingSpecies<T>(T[] startingSpecies, Action<T, Vector3> spawnAction)
    {
        if (startingSpecies != null && startingSpecies.Length > 0)
        {
            for (int i = 0; i < 5; i++)
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

    private List<Entity> GetNearbyEntities(Vector3 pos, float range, bool getAnimals, bool getPlants)
    {
        List<Entity> nearby = new List<Entity>();
        int cellRange = Mathf.CeilToInt(range / EnvironmentGrid.Instance.cellSize);

        Vector2Int center = EnvironmentGrid.Instance.GetCellCoords(pos);

        for (int x = -cellRange; x <= cellRange; x++)
        {
            for (int y = -cellRange; y <= cellRange; y++)
            {
                int gx = center.x + x;
                int gz = center.y + y;

                if (gx < 0 || gz < 0 || gx >= EnvironmentGrid.Instance.gridSize || gz >= EnvironmentGrid.Instance.gridSize)
                    continue;

                var cell = EnvironmentGrid.Instance.grid[gx, gz];
                if (getAnimals)
                {
                    nearby.AddRange(cell.animals);
                }
                if (getPlants)
                {
                    nearby.AddRange(cell.plants);
                }
            }
        }
        return nearby;
    }

    public List<Entity> GetNearbyEntities(Vector3 pos, float range)
    {
        return GetNearbyEntities(pos, range, true, true);
    }

    public List<Entity> GetNearbyAnimals(Vector3 pos, float range)
    {
        return GetNearbyEntities(pos, range, true, false);
    }

    public List<Entity> GetNearbyPlants(Vector3 pos, float range)
    {
        return GetNearbyEntities(pos, range, false, true);
    }
}
