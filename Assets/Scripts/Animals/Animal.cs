using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Animal : Entity
{
    public AnimalSpeciesData species;
    public AnimalState currentState;
    public Vector3 prevPosition;
    public float hunger, thirst, energy, matingDrive; //max 100, min 0
    public float age, stamina, health; //current values of the maximums in AnimalSpeciesData
    public float speciesLifespan; //max age
    public Entity target;
    public AnimalView view;

    public bool isRunning = false;
    public bool isWalking = false;

    private float decisionCooldown = 2f;
    private float timeSinceLastDecision = 0f;

    public Animal(AnimalSpeciesData species, Vector3 position, int Id)
    {
        base.id = Id;
        base.position = position;
        prevPosition = position;
        this.species = species;
        currentState = new IdleState();

        //Random lifespan
        float variation = species.lifespanVariation * species.lifespan;
        float randomizedLifespan = species.lifespan + Random.Range(-variation, variation);
        this.speciesLifespan = randomizedLifespan;

        hunger = 0;
        thirst = 0;
        energy = 100;
        matingDrive = 0;
        age = 0;
        stamina = species.stamina;
        health = species.maxHP;

        Debug.Log($"{species.speciesName} created with lifespan {speciesLifespan:F1}.");
    }

    public void UpdateAI(float timeStep)
    {
        //-----Core biological updates-----
        age += timeStep;
        hunger = Mathf.Min(100f, hunger + species.hungerRate * timeStep);
        thirst = Mathf.Min(100f, thirst + species.thirstRate * timeStep);
        //matingDrive = 
        
        if (isRunning)
        {
            stamina = Mathf.Max(0f, stamina - timeStep);
            energy -= species.energyDepletionRate * 2 * timeStep; // deplete energy faster while running
        }
        else if (isWalking)
        {
            stamina = Mathf.Min(species.stamina, stamina + timeStep);
            energy = Mathf.Max(0f, energy - species.energyDepletionRate * timeStep);
        }
        else
        {
            stamina = Mathf.Min(species.stamina, stamina + timeStep);
            energy = Mathf.Max(0f, energy - species.energyDepletionRate * 0.5f * timeStep);
        }

        //-----Health & Death management-----
        if (hunger >= 100f || thirst >= 100f || energy <= 0)
        {
            //starvation, exhaustion or dehydration damage
            health -= timeStep;
        }
        else
        {
            health = Mathf.Min(species.maxHP, health + species.hpRecoveryRate * timeStep);
        }

        if (health <= 0f || age > speciesLifespan)
        {
            Die();
            Debug.Log($"{species.speciesName} died. Age: {age:F1}");
            return;
        }

        //-----State update-----

        currentState?.Execute(this, timeStep);

        //-----Update Grid-----

        if ((int)(prevPosition.x) != (int)(position.x) || (int)(prevPosition.z) != (int)(position.z))
        {
            EnvironmentGrid.Instance.DeregisterAnimal(this);
            EnvironmentGrid.Instance.RegisterAnimal(this);
        }
    }

    public void EvaluateNeeds()
    {
        timeSinceLastDecision += WorldManager.Instance.timeStep;
        if (timeSinceLastDecision < decisionCooldown) return;

        timeSinceLastDecision = 0f;

        // Hunger/food seeking
        float hungerThreshold = 50f + Random.Range(-10f, 10f); // adds per-animal variation
        //float hungerProbability = Mathf.InverseLerp(hungerThreshold, 100f, hunger);

        if (hunger > hungerThreshold)
        {
            ChangeState(new SeekFoodState());
            return;
        }

        // Thirst/water seeking
        float thirstThreshold = 50f + Random.Range(-10f, 10f);
        float thirstProbability = Mathf.InverseLerp(thirstThreshold, 100f, thirst);

        if (Random.value < thirstProbability)
        {
            ChangeState(new SeekWaterState());
            return;
        }

        // Rest seeking
        float energyThreshold = 50f + Random.Range(-10f, 10f);
        float restProbability = Mathf.InverseLerp(energyThreshold, 0f, energy);

        if (Random.value < restProbability)
        {
            ChangeState(new SleepState());
            return;
        }

        // Mate seeking
        float matingThreshold = 50f + Random.Range(-10f, 10f);
        float matingProbability = Mathf.InverseLerp(matingThreshold, 0f, matingDrive);

        if (Random.value < matingProbability)
        {
            ChangeState(new SeekMateState());
            return;
        }
    }

    public void ChangeState(AnimalState newState)
    {
        // Sync the animal position with the visual position before changing states
        if (view != null)
        {
            Vector3 interpolated = view.GetInterpolatedPosition();
            prevPosition = interpolated;
            position = interpolated;
        }

        //Debug.Log("Changing State...");
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    public Entity FindNearestFood()
    {
        Vector3 pos = this.position;
        float range = species.awarenessRange;

        Entity nearest = null;

        if (species.isCarnivore)
        {
            Animal edible = null;

            foreach (var entity in WorldManager.Instance.GetNearbyAnimals(pos, range))
            {
                if (entity is Animal prey)
                {
                    if (prey == this) continue;

                    if (species.edibleAnimals != null && species.edibleAnimals.Length > 0)
                    {
                        foreach (var allowed in species.edibleAnimals)
                        {
                            if (prey.species == allowed)
                            {
                                edible = (Animal) ClosestEntity(edible, prey);
                            }
                        }
                    }
                    else
                    {
                        if (prey.species.dominance < species.dominance)
                            edible = (Animal) ClosestEntity(edible, prey);
                    }
                }
            }
        }
        else if (species.isHerbivore)
        {
            Plant edible = null;

            foreach (var entity in WorldManager.Instance.GetNearbyPlants(pos, range))
            {
                if (entity is Plant plant)
                {
                    if (species.edibleAnimals != null && species.edibleAnimals.Length > 0)
                    {
                        foreach (var allowed in species.edibleAnimals)
                        {
                            if (plant.species == allowed)
                            {
                                edible = (Plant)ClosestEntity(edible, plant);
                            }
                        }
                    }
                    else
                    {
                        if (plant.species.isEdible)
                            edible = (Plant)ClosestEntity(edible, plant);
                    }
                }
            }
        }
        return nearest;
    }

    private Entity ClosestEntity(Entity entityA, Entity entityB)
    {
        if(entityA == null)
        {
            return entityB;
        }
        if(entityB == null)
        {
            return entityA;
        }
        if (Vector3.Distance(position, entityA.position) < Vector3.Distance(position, entityB.position))
        {
            return entityA;
        }
        else
        {
            return entityB;
        }
    }

    public override void Die()
    {
        base.Die();
        AnimalManager.Instance.RemoveAnimal(this); //removes animal and view from lists and destroys gameobject
    }

    public bool ShouldRun(float timeStep)
    {
        return ((hunger < timeStep * species.hungerRate) &&
            (energy > species.energyDepletionRate * timeStep) &&
            (stamina >= 1));
    }
}
