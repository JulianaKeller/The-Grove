using UnityEngine;

public class WorldManager : MonoBehaviour
{
    //This script manages time steps and updates all systems

    public float timeStep = 0.1f;
    private float accumulator = 0f;
    private int tickCount = 0;

    void Start()
    {
        
    }

    void Update()
    {
        accumulator += Time.deltaTime;
        while (accumulator >= timeStep)
        {
            Tick(timeStep);
            accumulator -= timeStep;
            tickCount++;
        }
    }

    private void Tick(float dt)
    {
        EnvironmentManager.Instance.UpdateEnvironment(dt);
        PlantManager.Instance.UpdatePlants(dt, tickCount);
        AnimalManager.Instance.UpdateAnimals(dt, tickCount);
        //EventManager.Instance.UpdateEvents(dt);
    }
}
