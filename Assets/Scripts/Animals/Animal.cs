using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Animal : Entity
{
    public new AnimalSpeciesData species;
    public AnimalState currentState;
    public Vector3 prevPosition;
    public float hunger, thirst, energy, matingDrive; //max 100, min 0
    public float stamina; //current values of the maximums in AnimalSpeciesData
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
    public bool isEating = false;
    public bool isDrinking = false;

    private float decisionCooldown = 2f;
    private float timeSinceLastDecision = 0f;

    public Animator animator;

    public Animal(AnimalSpeciesData species, Vector3 position, int Id) : base(species)
    {
        base.id = Id;
        base.isAnimal = true;
        base.position = position;
        prevPosition = position;
        this.species = species;
        currentState = new IdleState();

        base.setLifespan();

        dominance = species.baseDominance + Random.Range(-species.dominanceVariation, species.dominanceVariation);

        hunger = 0;
        thirst = 0;
        energy = 100;
        matingDrive = 0;
        stamina = species.stamina;
        health = species.maxHP;
        isFemale = Random.value >= 0.5f;
    }

    public void UpdateAI(float timeStep)
    {
        if (isAlive)
        {
            BiologicalUpdates(timeStep);

            UpdateHealth(timeStep);

            if (isAlive) //Check again if still alive
            {
                EvaluateNeeds(timeStep);

                PerceptionCheck();

                currentState?.Execute(this, timeStep);

                updateAnimations();

                //-----Update Grid-----

                if ((int)(prevPosition.x) != (int)(position.x) || (int)(prevPosition.z) != (int)(position.z))
                {
                    EnvironmentGrid.Instance.DeregisterAnimal(this);
                    EnvironmentGrid.Instance.RegisterAnimal(this);
                }
            }
        }
        else
        {
            nutritionValue -= 0.2f * timeStep;
                
            if(nutritionValue <= 0)
            {
                AnimalManager.Instance.RemoveAnimal(this);
            }
        }
    }

    public void ChangeState(AnimalState newState)
    {
        if (newState == currentState)
        {
            return;
        }

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
            health = Mathf.Max(0, health - timeStep);
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
        float healthFactor = Mathf.Clamp01(health / species.maxHP);
        float hungerFactor = Mathf.Clamp01(hunger / 100);
        bool canEatEnemy = false;

        if (species.isCarnivore)
        {
            // check if enemy is edible (same logic as your FindNearestFood)
            if (species.edibleAnimals != null && species.edibleAnimals.Length > 0)
            {
                foreach (var allowed in species.edibleAnimals)
                {
                    if (enemy.species == allowed)
                    {
                        canEatEnemy = true;
                        break;
                    }
                }
            }
            else
            {
                if (enemy.dominance < dominance)
                    canEatEnemy = true;
            }
        }

        if (!canEatEnemy)
        {
            hunger = 0;
        }

        float fightScore = 0f;

        fightScore += species.aggression * 0.3f;
        fightScore += healthFactor * 0.3f;
        fightScore += hungerFactor * 0.3f;
        if (dominance > enemy.dominance)
            fightScore += 0.3f;

        if (fightScore >= 0.5f)
            ChangeState(new FightState());
        else
            ChangeState(new FleeState());
    }

    public void MoveTo(Vector3 targetPos, float timeStep)
    {
        if (!IsValidVector(targetPos))
        {
            targetPos = position; //fallback to current position
        }

        float maxX = EnvironmentGrid.Instance.gridCenter.x + EnvironmentGrid.Instance.gridSize * EnvironmentGrid.Instance.cellSize * 0.5f;
        float maxZ = EnvironmentGrid.Instance.gridCenter.z + EnvironmentGrid.Instance.gridSize * EnvironmentGrid.Instance.cellSize * 0.5f;
        float minX = EnvironmentGrid.Instance.gridCenter.x - EnvironmentGrid.Instance.gridSize * EnvironmentGrid.Instance.cellSize * 0.5f;
        float minZ = EnvironmentGrid.Instance.gridCenter.z - EnvironmentGrid.Instance.gridSize * EnvironmentGrid.Instance.cellSize * 0.5f;

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

        float speed = 0;

        if (ShouldRun(timeStep))
        {
            speed = species.runningSpeed;
            isRunning = true;
        }
        else
        {
            speed = species.walkingSpeed;
            isWalking = true;
        }

            Vector3 newPos = Vector3.MoveTowards(position, targetPos, speed * timeStep);
        prevPosition = position;
        position = newPos;

        if (view != null)
            view.FaceTowards(newPos);
    }

    private bool IsValidVector(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsInfinity(v.x) ||
                 float.IsNaN(v.y) || float.IsInfinity(v.y) ||
                 float.IsNaN(v.z) || float.IsInfinity(v.z));
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
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isEating", false);
        animator.SetBool("isDrinking", false);
        animator.SetBool("isSleeping", false);
        animator.SetBool("isFighting", false);

        if (isRunning)
        {
            animator.SetBool("isRunning", true);
        }
        else if (isWalking)
        {
            animator.SetBool("isWalking", true);
        }
        else if (isSleeping)
        {
            animator.SetBool("isSleeping", true);
        }
        else if (isEating)
        {
            animator.SetBool("isEating", true);
        }
        else if (isDrinking)
        {
            animator.SetBool("isDrinking", true);
        }
        else if (isFighting)
        {
            animator.SetBool("isFighting", true);
        }
        if(health <= 0)
        {
            animator.SetBool("isDead", true);
        }
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
                                //ToDo take into account if larger nutrition value and lower dominance & health than current edible
                                
                                edible = (Animal) ClosestEntity(edible, prey); //ToDo prefer also already dead animals over alive ones
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
            nearest = edible;
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
            nearest = edible;
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

        currentState = null;
        StopBehaviors();

        GetNutritionValue();

        //AnimalManager.Instance.RemoveAnimal(this); //removes animal and view from lists and destroys gameobject
    }

    public bool ShouldRun(float timeStep)
    {
        //ToDo dont run if low on hunger, energy, health
        return (stamina >= 1) && 
                ((isFleeing) ||
                ((hunger < timeStep * species.hungerRate) &&
                (energy > species.energyDepletionRate * timeStep)));
    }

    private void StopBehaviors()
    {
        isRunning = false;
        isWalking = false;
        isFleeing = false;
        isFighting = false;
        isSleeping = false;
        isMating = false;
        isEating = false;
        isDrinking = false;

        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isEating", false);
        animator.SetBool("isDrinking", false);
        animator.SetBool("isSleeping", false);
        animator.SetBool("isMating", false);
        animator.SetBool("isFighting", false);
        animator.SetBool("isFleeing", false);
    }
}
