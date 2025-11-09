using System.Collections.Generic;
using Unity.VisualScripting;
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
    public int dominance;
    public Entity target;
    public AnimalView view;
    public Animal mother;
    //public Animal[] herd; //better way to save?
    public bool isFemale = true;

    public bool isRunning = false;
    public bool isWalking = false;
    public bool isFleeing = false;
    public bool isFighting = false;
    public bool isSleeping = false;
    public bool isMating = false;

    private float decisionCooldown = 2f;
    private float timeSinceLastDecision = 0f;

    public Animator animator;

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

        dominance = species.baseDominance + Random.Range(-species.dominanceVariation, species.dominanceVariation);

        hunger = 0;
        thirst = 0;
        energy = 100;
        matingDrive = 0;
        age = 0;
        stamina = species.stamina;
        health = species.maxHP;
        isFemale = Random.value >= 0.5f;

        Debug.Log($"{species.speciesName} created with lifespan {speciesLifespan:F1}.");
    }

    public void UpdateAI(float timeStep)
    {
        BiologicalUpdates(timeStep);

        updateAnimations();

        UpdateHealth(timeStep);

        EvaluateNeeds(timeStep);

        PerceptionCheck();

        currentState?.Execute(this, timeStep);

        //-----Update Grid-----

        if ((int)(prevPosition.x) != (int)(position.x) || (int)(prevPosition.z) != (int)(position.z))
        {
            EnvironmentGrid.Instance.DeregisterAnimal(this);
            EnvironmentGrid.Instance.RegisterAnimal(this);
        }
    }

    private void BiologicalUpdates(float timeStep)
    {
        age += timeStep;
        hunger = Mathf.Min(100f, hunger + species.hungerRate * timeStep);
        thirst = Mathf.Min(100f, thirst + species.thirstRate * timeStep);
        matingDrive = Mathf.Min(100f, matingDrive + 0.1f * timeStep); //ToDo dependent on age see plants

        if (isRunning)
        {
            stamina = Mathf.Max(0f, stamina - timeStep);
            energy = Mathf.Max(0f, energy - species.energyDepletionRate * 2 * timeStep);
        }
        else if (isWalking)
        {
            stamina = Mathf.Min(species.stamina, stamina + timeStep);
            energy = Mathf.Max(0f, energy - species.energyDepletionRate * timeStep);
        }
        else
        {
            stamina = Mathf.Min(species.stamina, stamina + timeStep);
            energy = Mathf.Min(100f, energy + species.energyDepletionRate * 0.1f * timeStep);
        }
    }

    private void UpdateHealth(float timeStep)
    {
        if (hunger >= 100f || thirst >= 100f || energy <= 0)
        {
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
    }

    public void PerceptionCheck()
    {
        Animal nearestThreat = ClosestEntity(GetNearbyThreats()) as Animal;

        if (nearestThreat == null)
        {
            return;
        }

        float perceptionChance = isSleeping ? 0.2f : 0.9f;
        float distance = Vector3.Distance(position, nearestThreat.position);
        float distanceModifier = Mathf.Clamp01(1f - (distance / species.awarenessRange));
        perceptionChance = perceptionChance * distanceModifier;

        if (Random.value > perceptionChance)
            return;

        if (nearestThreat != null )
        {
            FightOrFlight(nearestThreat);
        }
    }

    public void FightOrFlight(Animal enemy)
    {
        //ToDo health should be an influence
        if(dominance > enemy.dominance && species.aggression > 0.4f) //ToDo add some randomness/probability calculations, closeness to enemy should be an influence
        {
            ChangeState(new FightState());
        }
        else if(species.aggression > 0.6f)
        {
            ChangeState(new FightState());
        }
        else
        {
            ChangeState(new FleeState());
        }
    }

    public void MoveTo(Vector3 targetPos, float timeStep)
    {
        float speed = (ShouldRun(timeStep) ? species.runningSpeed : species.walkingSpeed);

        Vector3 newPos = Vector3.MoveTowards(position, targetPos, speed * timeStep);
        prevPosition = position;
        position = newPos;

        if (view != null)
            view.FaceTowards(newPos);
    }
    
    public void EvaluateNeeds(float timeStep)
    {
        if(isFleeing || isFighting)
        {
            return;
        }

        timeSinceLastDecision += timeStep;
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
        //float thirstProbability = Mathf.InverseLerp(thirstThreshold, 100f, thirst);

        if (thirst > thirstThreshold)
        {
            ChangeState(new SeekWaterState());
            return;
        }

        // Rest seeking
        float energyThreshold = 50f + Random.Range(-10f, 10f);
        //float restProbability = Mathf.InverseLerp(energyThreshold, 0f, energy);

        if (energy < energyThreshold)
        {
            ChangeState(new SleepState());
            return;
        }

        // Mate seeking
        float matingThreshold = 50f + Random.Range(-10f, 10f);
        //float matingProbability = Mathf.InverseLerp(matingThreshold, 100f, matingDrive);

        if (matingDrive > matingThreshold)
        {
            ChangeState(new SeekMateState());
            return;
        }
    }

    public void updateAnimations()
    {
        if (animator == null)
        {
            return;
        }
        if (isRunning)
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isWalking", false);
        }
        else if (isWalking)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", false);
        }
    }

    public void ChangeState(AnimalState newState)
    {
        isRunning = false;
        isWalking = false;

        // Sync the animal position with the visual position before changing states
        if (view != null)
        {
            Vector3 interpolated = view.GetInterpolatedPosition();
            prevPosition = interpolated;
            position = interpolated;
        }

        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    public Entity FindNearestFood()
    {
        Vector3 pos = this.position;

        Entity nearest = null;

        if (species.isCarnivore)
        {
            Animal edible = null;

            foreach (var entity in WorldManager.Instance.GetNearbyAnimals(pos, species.awarenessRange))
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
                        if (prey.dominance < dominance && prey.species != species)
                            edible = (Animal) ClosestEntity(edible, prey);
                    }
                }
            }
        }
        else if (species.isHerbivore)
        {
            Plant edible = null;

            foreach (var entity in WorldManager.Instance.GetNearbyPlants(pos, species.awarenessRange))
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

    private Entity ClosestEntity<T>(List<T> entities) where T : Entity
    {
        Entity closest = null;

        foreach (var entity in entities)
        {
            if(entity == this)
            {
                continue;
            }
            closest = ClosestEntity(closest, entity);
        }

        return closest;
    }

    private List<Animal> GetNearbyThreats()
    {
        List<Animal> threats = new List<Animal>();
        foreach(Animal a in WorldManager.Instance.GetNearbyPredators(position, species.awarenessRange))
        {
            if (IsThreat(a))
            {
                threats.Add(a);
            }
        }
        return threats;
    }

    public bool IsThreat(Animal other)
    {
        return species.fearedAnimals != null && species.fearedAnimals.Contains(other.species);
    }

    public override void Die()
    {
        base.Die();
        AnimalManager.Instance.RemoveAnimal(this); //removes animal and view from lists and destroys gameobject
    }

    public bool ShouldRun(float timeStep)
    {
        //ToDo dont run if low on hunger, energy, health
        return (stamina >= 1) && 
                ((isFleeing) ||
                ((hunger < timeStep * species.hungerRate) &&
                (energy > species.energyDepletionRate * timeStep)));
    }
}
