using System.Drawing;
using UnityEngine;

public class Plant : Entity
{
    public PlantSpeciesData species;
    public float growth;
    public float age;
    public float size;

    public void UpdatePlant(float timeStep)
    {
        age += timeStep;
        size += species.growthRate * timeStep;
        if (age > species.maxAge)
            Die();
        //ToDo execute states
    }

    public override void Die()
    {
        base.Die();
        PlantManager.Instance.RemovePlant(this);
    }
}
