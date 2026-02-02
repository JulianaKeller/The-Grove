using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class Animal : Entity
{
    public enum AnimalVisualState
    {
        Idle,
        Wandering,
        SearchingFoodCarnivore,
        SearchingFoodHerbivore,
        SearchingWater,
        SearchingMate,
        Eating,
        Drinking,
        Sleeping,
        Fighting,
        Fleeing,
        Following,
        Mating,
        Dead
    }

    [Header("State")]
    public AnimalState currentState;

    [Header("Movement")]
    public Vector3 prevPosition = Vector3.zero;
    public Vector3 targetPosition = Vector3.zero;

    [Header("Bezier Curve Movement")]
    private Vector3 pathStart;
    private Vector3 pathControl;
    private Vector3 pathEnd;
    private float pathT;
    public bool hasPath;
    public Vector3 facingDirection = Vector3.forward;

    [Header("Dynamic Stats")]
    public float hunger, thirst, energy, matingDrive; //max 100, min 0
    public float stamina; //current values of the maximums in AnimalSpeciesData
    public int dominance;
    public Entity target;
    //public Herd herd; //ToDo implement Herd

    [Header("Static Stats")]
    public new AnimalSpeciesData species;
    public bool isFemale = true;
    public AnimalView view;
    public Animal mother;

    [Header("Bools")]
    public bool isRunning = false;
    public bool isWalking = false;
    public bool isFleeing = false;
    public bool isFighting = false;
    public bool isSleeping = false;
    public bool isMating = false;
    public bool isEating = false;
    public bool isDrinking = false;

    [Header("Last Found")]
    public Vector3 lastFoundFoodPos = Vector3.zero;
    public Vector3 lastFoundWaterPos = Vector3.zero;
    public Vector3 lastFoundMatePos = Vector3.zero;

    [Header("Migration")]
    public int migrationThreshold = 5;
    public int failedFoodSearches = 0;
    public int failedWaterSearches = 0;
    public int failedMateSearches = 0;

    [Header("Follow Behavior")]
    private float maturityThreshold;
    public float followDistance = 6f; // preferred max distance from mother

    public Animator animator;

    public UnityEvent<AnimalVisualState> OnStateChanged = new UnityEvent<AnimalVisualState>();

    public Animal(AnimalSpeciesData species, Vector3 position, int Id, Animal mother) : base(species, Id)
    {
        base.isAnimal = true;
        base.position = position;
        prevPosition = position;
        this.species = species;
        this.mother = mother;
        currentState = new IdleState();
        NotifyStateChange();

        base.setLifespan();
        maturityThreshold = speciesLifespan * 0.25f;

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
                EvaluateShortTermNeeds(timeStep);

                currentState?.Execute(this, timeStep);

                MoveToTarget(timeStep);

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

    #region Updates

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
            energy = Mathf.Max(0f, energy - species.energyDepletionRate * 0.1f * timeStep);
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

    public override void Die()
    {
        base.Die();

        currentState = null;
        StopBehaviors();

        GetNutritionValue();

        //AnimalManager.Instance.RemoveAnimal(this); //removes animal and view from lists and destroys gameobject
    }

    #endregion

    #region Perception and Reflexes

    public void PerceptionCheck()
    {
        //ToDo Is this efficient enough?

        Animal nearestThreat = GetNearbyThreat();

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

        if (nearestThreat != null)
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
            if (species.edibleEntities != null && species.edibleEntities.Length > 0)
            {
                foreach (var allowed in species.edibleEntities)
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
            ChangeState(new FightState(enemy));
        else
            ChangeState(new FleeState(enemy));
    }

    public void EvaluateShortTermNeeds(float timeStep)
    {
        if (isFleeing || isFighting) //finish these states before switching according to needs
        {
            return;
        }

        //Threat perception check:
        PerceptionCheck();

        EvaluateFollowBehavior(timeStep);
    }

    #endregion

    #region State Change

    public bool EvaluateLongTermNeeds()
    {
        bool changedState = false;
        if (failedFoodSearches > migrationThreshold)
        {
            ChangeState(new MigrateState(MigrateState.MigrationReason.Food));
            changedState = true;
            return changedState;
        }
        if (failedWaterSearches > migrationThreshold)
        {
            ChangeState(new MigrateState(MigrateState.MigrationReason.Water));
            changedState = true;
            return changedState;
        }
        if (failedMateSearches > migrationThreshold)
        {
            ChangeState(new MigrateState(MigrateState.MigrationReason.Mate));
            changedState = true;
            return changedState;
        }

        // Hunger/food seeking
        float hungerThreshold = 50f + Random.Range(-10f, 10f);

        if (hunger > hungerThreshold)
        {
            ChangeState(new SeekFoodState());
            changedState = true;
            return changedState;
        }

        // Thirst/water seeking
        float thirstThreshold = 50f + Random.Range(-10f, 10f);

        if (thirst > thirstThreshold)
        {
            ChangeState(new SeekWaterState());
            changedState = true;
            return changedState;
        }

        // Rest seeking
        float energyThreshold = 50f + Random.Range(-10f, 10f);

        if (energy < energyThreshold)
        {
            ChangeState(new SleepState());
            changedState = true;
            return changedState;
        }

        if (age > maturityThreshold)
        {
            // Mate seeking
            float matingThreshold = 50f + Random.Range(-10f, 10f);

            if (matingDrive > matingThreshold)
            {
                ChangeState(new SeekMateState());
                changedState = true;
                return changedState;
            }
        }
        return changedState;
    }

    private void EvaluateFollowBehavior(float timeStep)
    {
        if (age > maturityThreshold && mother != null && mother.isAlive)
        {

            float dist = Vector3.Distance(position, mother.position);
            if (dist > followDistance)
            {
                float followProbability = (dist - followDistance) * 0.1f;
                followProbability = Mathf.Clamp01(followProbability);

                if (UnityEngine.Random.value < followProbability)
                {
                    ChangeState(new FollowState(mother));
                    return;
                }
            }
        }

        //Follow herd logic can be added here later
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

        hasPath = false;
        pathT = 0f;

        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);

        NotifyStateChange();
    }

    private void NotifyStateChange()
    {
        AnimalVisualState visualState = ResolveVisualState();
        OnStateChanged.Invoke(visualState);
    }

    private AnimalVisualState ResolveVisualState()
    {
        if (!isAlive) return AnimalVisualState.Dead;
        if (currentState is FightState) return AnimalVisualState.Fighting;
        if (currentState is FleeState) return AnimalVisualState.Fleeing;
        if (currentState is FollowState) return AnimalVisualState.Following;
        if (currentState is SeekFoodState && species.isCarnivore) return AnimalVisualState.SearchingFoodCarnivore;
        if (currentState is SeekFoodState && species.isHerbivore) return AnimalVisualState.SearchingFoodHerbivore;
        if (currentState is SeekWaterState) return AnimalVisualState.SearchingWater;
        if (currentState is SeekMateState) return AnimalVisualState.SearchingMate;
        if (currentState is EatState) return AnimalVisualState.Eating;
        if (currentState is DrinkState) return AnimalVisualState.Drinking;
        if (currentState is SleepState) return AnimalVisualState.Sleeping;
        if (currentState is MateState) return AnimalVisualState.Mating;
        if (currentState is WanderState) return AnimalVisualState.Wandering;
        return AnimalVisualState.Idle;
    }

    public void updateAnimations()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isEating", isEating);
        animator.SetBool("isDrinking", isDrinking);
        animator.SetBool("isSleeping", isSleeping);
        animator.SetBool("isFighting", isFighting);

        if (health <= 0)
        {
            animator.SetBool("isDead", true);
        }
    }

    #endregion

    #region Movement

    public void SetMoveTarget(Vector3 targetPos)
    {
        ValidateTargetPosition(targetPos);

        pathStart = position;
        pathEnd = targetPosition;

        Vector3 forward =
        (position - prevPosition).sqrMagnitude > 0.001f
        ? (position - prevPosition).normalized
        : facingDirection;

        float distance = Vector3.Distance(pathStart, pathEnd);
        pathControl = pathStart + forward * distance * 0.5f;

        pathT = 0f;
        hasPath = true;
    }

    private void ValidateTargetPosition(Vector3 targetPos)
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

        this.targetPosition = targetPos;
    }

    public void MoveToTarget(float timeStep)
    {
        if (!hasPath)
            return;

        float speed;
        if (ShouldRun(timeStep))
        {
            speed = species.runningSpeed;
            isRunning = true;
            isWalking = false;
        }
        else
        {
            speed = species.walkingSpeed;
            isWalking = true;
            isRunning = false;
        }

        float distanceToMove = speed * timeStep;

        pathT = AdvanceTByDistance(pathT, distanceToMove);

        prevPosition = position;
        position = GetBezierPosition(pathStart, pathControl, pathEnd, pathT);

        Vector3 tangent = GetBezierTangent(pathStart, pathControl, pathEnd, pathT);
        if (tangent.sqrMagnitude > 0.0001f)
            facingDirection = tangent.normalized;

        FaceTowards(position);

        if (pathT >= 1f)
        {
            hasPath = false;
            position = pathEnd;
        }
    }

    //This function converts "move X meters" -> "how far should t advance on the curve"
    private float AdvanceTByDistance(float t, float distance)
    {
        const int maxIterations = 20;

        float remaining = distance;
        Vector3 prev = GetBezierPosition(pathStart, pathControl, pathEnd, t);

        for (int i = 0; i < maxIterations && remaining > 0f && t < 1f; i++)
        {
            float dt = Mathf.Min(0.05f, 1f - t);
            float nextT = t + dt;

            Vector3 next = GetBezierPosition(pathStart, pathControl, pathEnd, nextT);
            float segmentLength = Vector3.Distance(prev, next);

            if (segmentLength > remaining)
            {
                dt *= remaining / segmentLength;
                nextT = t + dt;
                next = GetBezierPosition(pathStart, pathControl, pathEnd, nextT);
                segmentLength = remaining;
            }

            remaining -= segmentLength;
            t = nextT;
            prev = next;
        }

        return t;
    }


    public Vector3 GetBezierPosition(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        float u = 1 - t;
        return u * u * start + 2 * u * t * control + t * t * end;
    }

    private static Vector3 GetBezierTangent(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return
            2f * (1f - t) * (b - a) +
            2f * t * (c - b);
    }

    public void FaceTowards(Vector3 newPos)
    {
        if (view != null)
            view.FaceTowards(newPos);
    }

    public void FaceTowardsImmediate(Vector3 newPos)
    {
        if (view != null)
            view.FaceTowardsImmediate(newPos);
    }

    public bool ShouldRun(float timeStep)
    {
        //ToDo dont run if low on hunger, energy, health
        return (stamina >= timeStep) &&
                ((isFleeing) ||
                ((hunger < timeStep * species.hungerRate) &&
                (energy > species.energyDepletionRate * timeStep)));
    }

    #endregion

    #region Finding Nemo <3

    public Entity FindNearestFood()
    {
        Entity food = null;
        if (species.isCarnivore && species.isHerbivore)
        {
            food = Closest(WorldManager.Instance.GetNearbyEntities(this.position, species.awarenessRange), FilterEdible);
        }
        else if (species.isCarnivore)
        {
            food = Closest(WorldManager.Instance.GetNearbyAnimals(this.position, species.awarenessRange), FilterEdible);
        }
        else if (species.isHerbivore)
        {
            food = Closest(WorldManager.Instance.GetNearbyPlants(this.position, species.awarenessRange), FilterEdible);
        }

        if (food == null)
        {
            failedFoodSearches++;
        }
        else
        {
            lastFoundFoodPos = food.position;
            failedFoodSearches = 0;
        }
        return food;
    }

    public WaterSource FindNearestWaterSource()
    {
        WaterSource ws = Closest(WorldManager.Instance.GetNearbyWaterSources(this.position, species.awarenessRange));

        if (ws == null)
        {
            failedWaterSearches++;
        }
        else
        {
            lastFoundWaterPos = ws.position; //NullReferenceException!
            failedWaterSearches = 0;
        }
        return ws;
    }

    private Animal GetNearbyThreat()
    {
        Entity threat = Closest(WorldManager.Instance.GetNearbyAnimals(this.position, species.awarenessRange), IsThreat);
        return threat as Animal;
    }

    public Animal GetNearbyMate()
    {
        Entity mate = Closest(WorldManager.Instance.GetNearbyAnimals(this.position, species.awarenessRange), IsMate);

        if (mate == null)
        {
            failedMateSearches++;
        }
        else
        {
            lastFoundMatePos = mate.position;
            failedMateSearches = 0;
        }
        return mate as Animal;
    }

    private Entity Closest(IEnumerable<Entity> entities)
    {
        return Closest(entities, _ => true);
    }

    private Entity Closest(IEnumerable<Entity> entities, Func<Entity, bool> filter)
    {
        Entity closest = null;

        foreach (var entity in entities)
        {
            if (filter(entity))
            {
                closest = Closest(closest, entity); ;
            }
        }

        return closest;
    }

    private WaterSource Closest(List<WaterSource> waterSources)
    {
        WaterSource closest = null;

        foreach (var ws in waterSources)
        {
            closest = Closest(closest, ws);
        }
        return closest;
    }

    private Entity Closest(Entity entityA, Entity entityB)
    {
        if (entityA == null)
        {
            return entityB;
        }
        if (entityB == null)
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

    private WaterSource Closest(WaterSource wsA, WaterSource wsB)
    {
        if (wsA == null)
        {
            return wsB;
        }
        if (wsB == null)
        {
            return wsA;
        }
        if (Vector3.Distance(position, wsA.position) < Vector3.Distance(position, wsB.position))
        {
            return wsA;
        }
        else
        {
            return wsB;
        }
    }

    #endregion

    #region Entity Filters

    private bool FilterEdible(Entity entity)
    {
        bool isEdible = false;

        if (species.edibleEntities != null && species.edibleEntities.Contains(entity.species))
        {
            isEdible = true;
        }
        else
        {
            if (species.isCarnivore)
            {
                if (entity is Animal prey)
                {
                    if (prey.dominance < dominance && prey.species != species && prey != this)
                    {
                        isEdible = true;
                    }
                }
            }
            if (species.isHerbivore)
            {
                if (entity is Plant plant)
                {
                    isEdible = plant.species.isEdible;
                }
            }
        }

        return isEdible;
    }

    public bool IsMate(Entity other)
    {
        if (other is Animal a)
        {
            return (a.species == this.species) && (a.isFemale != this.isFemale) && (a.age >= a.maturityThreshold) && (this.age >= maturityThreshold);
        }
        return false;
    }

    public bool IsThreat(Entity other)
    {
        if (other is Animal a)
        {
            return a.species.isCarnivore && species.fearedAnimals != null && species.fearedAnimals.Contains(other.species);
        }
        return false;
    }

    #endregion

    #region Utilities

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

    private bool IsValidVector(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsInfinity(v.x) ||
                 float.IsNaN(v.y) || float.IsInfinity(v.y) ||
                 float.IsNaN(v.z) || float.IsInfinity(v.z));
    }

    #endregion

    #region Overrides

    public static bool operator ==(Animal a, Animal b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;

        return a.id == b.id;
    }

    public static bool operator !=(Animal a, Animal b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null) return false;

        if (obj is Animal other)
            return id == other.id;

        return false;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    #endregion
}
