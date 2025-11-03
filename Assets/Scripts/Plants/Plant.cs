using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class Plant : Entity
{
    public PlantSpeciesData species;
    public float age;
    public Vector3 size;
    public Vector3 prevSize;
    public float health;
    public float speciesLifespan;

    public PlantView view;

    public Plant(PlantSpeciesData species, Vector3 position, int Id)
    {
        base.id = Id;
        base.position = position;
        this.species = species;

        float variation = species.lifespanVariation * species.lifespan;
        float randomizedLifespan = species.lifespan + Random.Range(-variation, variation);
        this.speciesLifespan = randomizedLifespan;

        age = 0;
        size = new Vector3(0,0,0);
        prevSize = new Vector3(0, 0, 0);
        health = 100;

        Debug.Log("Animal created.");
    }

    public void UpdatePlant(float timeStep)
    {
        age += timeStep;
        prevSize = size;
        float growth = species.growthRate * timeStep;
        size += Vector3.one * growth; //Use a more natural, nonlinear growth pattern

        if (age > speciesLifespan) //ToDo plant should also die if health reaches 0
        {
            Die();
            Debug.Log(species.name + " died.");
        }

        //ToDo execute states
    }

    public override void Die()
    {
        base.Die();
        PlantManager.Instance.RemovePlant(this);
    }
}
