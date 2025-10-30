using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Animal : Entity
{
    public AnimalSpeciesData species;
    public AnimalState currentState;
    public Vector3 prevPosition;
    public float hunger, thirst, energy, age;
    public Entity target;
    public AnimalView view;

    public Animal(AnimalSpeciesData species, Vector3 position, int Id)
    {
        base.id = Id;
        base.position = position;
        prevPosition = position;
        this.species = species;
        currentState = new IdleState(); //ToDo correct?

        hunger = 0;
        thirst = 0;
        energy = 100;
        age = 0;

        Debug.Log("Animal created.");
    }

    public void UpdateAI(float timeStep)
    {
        age += timeStep;
        hunger += species.hungerRate * timeStep;
        thirst += species.thirstRate * timeStep;
        energy -= 0.1f * timeStep;

        if(age > species.lifespan) //ToDo Animal should also die if hunger or thirst reaches 100
        {
            Die();
            Debug.Log(species.name + " died.");
        }

        currentState?.Execute(this, timeStep);
    }

    public void ChangeState(AnimalState newState)
    {
        Debug.Log("Changing State...");
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    Entity FindNearestFood(Vector3 pos, float range)
    {
        Entity nearest = null;
        float minDist = range;

        foreach (var entity in WorldManager.Instance.GetNearbyEntities(pos, range))
        {
            if (entity is Plant p && p.isAlive)
            {
                float dist = Vector3.Distance(pos, p.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = p;
                }
            }
        }
        return nearest;
    }

    public override void Die()
    {
        base.Die();
        AnimalManager.Instance.RemoveAnimal(this); //removes animal and view from lists and destroys gameobject
    }
}
