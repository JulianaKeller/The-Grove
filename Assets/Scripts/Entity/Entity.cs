using UnityEngine;

public abstract class Entity
{
    public int id { get; private set; }
    public Vector3 position; //current position
    public EntitySpeciesData species;
    public bool isAlive = true;
    public bool isAnimal = false;
    public bool isBeingEaten = false;

    public float nutritionValue;
    public float health;
    public float maxHealth;
    public float speciesLifespan;
    public float age;

    public Vector3 size;
    public Vector3 prevSize;
    public Vector3 maxSize;
    public Vector3 minSize;

    public virtual void UpdateEntity(float dt, bool canGrow) {
        age += dt;
        if (canGrow && isAlive)
        {
            prevSize = size;

            UpdateGrowth();
        }
        if (canGrow)
        {
            UpdateNutritionValue();
        }
        UpdateMaxHealth();
    }

    protected virtual void UpdateGrowth()
    {
        float effectiveAge = age * species.growthRate;

        float age01 = Mathf.Clamp01(effectiveAge / (speciesLifespan * 0.25f));

        size = Vector3.Lerp(minSize, maxSize, age01);
    }

    public Entity(EntitySpeciesData species, int Id)
    {
        id = Id;
        age = 0;
        nutritionValue = 0f;
        health = 100;
        this.species = species;

        setLifespan();

        UpdateMaxHealth();
        health = maxHealth;

        EcosystemMetrics.Instance.RegisterSpawn(species);
    }

    public void InitializeSizeValues(GameObject original)
    {
        maxSize = original.transform.localScale;
        float variation = Random.Range(species.maxSizeVariation.x, species.maxSizeVariation.y);
        maxSize = maxSize * variation;

        minSize = maxSize * 0.5f;
        if(this is Plant)
        {
            minSize = maxSize * 0.3f;
        }

        size = minSize;
        prevSize = minSize;
    }

    public virtual void setLifespan()
    {
        float variation = species.lifespanVariation * species.lifespan;
        float randomizedLifespan = species.lifespan + Random.Range(-variation, variation);
        this.speciesLifespan = randomizedLifespan;
    }

    public void BeEaten(float timeStep)
    {
        UpdateNutritionValue();
        nutritionValue -= 2f * timeStep;
        nutritionValue = Mathf.Max(nutritionValue, 0f);

        if (nutritionValue <= 0)
        {
            if (this is Plant)
            {
                Die();
            }
            else if (this is Animal)
            {
                AnimalManager.Instance.RemoveAnimal(this as Animal);
            }
        }
    }

    public void UpdateNutritionValue()
    {
        /*Nutrition follows a bell curve (max at 25% of lifespan age)*/

        float gaussianNew = GetGaussianAgeFactor();
        float gaussianPrev = GetGaussianAgeFactor(Mathf.Max(age - WorldManager.Instance.timeStep, 0));

        float newNutritionValue = species.nutritionBaseValue * (0.4f + gaussianNew);
        float previousNutritionValue = species.nutritionBaseValue * (0.4f + gaussianPrev);
        nutritionValue += (newNutritionValue - previousNutritionValue);

    }

    public void UpdateMaxHealth()
    {
        float gaussianNew = GetGaussianAgeFactor();

        maxHealth = species.maxHP * (0.4f + gaussianNew);
    }

    public float GetGaussianAgeFactor() //returns values between 0 and 1, with a bell curve peaking at 25% of lifespan
    {
        float ageFactor = Mathf.Clamp01(age / speciesLifespan);
        float peakAge = 0.25f;

        return Mathf.Exp(-Mathf.Pow((ageFactor - peakAge) / 0.25f, 2f));
    }

    public float GetGaussianAgeFactor(float age)
    {
        float ageFactor = Mathf.Clamp01(age / speciesLifespan);
        float peakAge = 0.25f;

        return Mathf.Exp(-Mathf.Pow((ageFactor - peakAge) / 0.25f, 2f));
    }

    public virtual void Die()
    {
        isAlive = false;
        EcosystemMetrics.Instance.RegisterDeath(species);
    }
    public static bool operator ==(Entity a, Entity b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;

        return a.id == b.id;
    }

    public static bool operator !=(Entity a, Entity b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null) return false;

        if (obj is Entity other)
            return id == other.id;

        return false;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }
}
