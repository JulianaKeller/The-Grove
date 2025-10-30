using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class WorldManager : MonoBehaviour
{
    //This script manages time steps and updates all systems
    public static WorldManager Instance { get; private set; }

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
        EnvironmentGrid.Instance.UpdateGrid(timeStep);
        PlantManager.Instance.UpdatePlants(timeStep, tickCount);
        AnimalManager.Instance.UpdateAnimals(timeStep, tickCount);
        //EventManager.Instance.UpdateEvents(timeStep);
    }

    public List<Entity> GetNearbyEntities(Vector3 pos, float range)
    {
        return new List<Entity>();
    }
}
