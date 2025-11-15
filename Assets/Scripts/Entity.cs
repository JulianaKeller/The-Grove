using UnityEngine;

public abstract class Entity
{
    public int id;
    public Vector3 position; //current position
    public EntitySpeciesData species;
    public bool isAlive = true;
    public bool isAnimal = false;
    public bool isBeingEaten = false;

    protected float nutritionValue;
    public float health;
    public float speciesLifespan;
    public float age;

    public virtual void UpdateEntity(float dt) { }

    public Entity(EntitySpeciesData species)
    {
        age = 0;
        nutritionValue = 0f;
        health = 100;
        this.species = species;
    }

    public virtual void setLifespan()
    {
        float variation = species.lifespanVariation * species.lifespan;
        float randomizedLifespan = species.lifespan + Random.Range(-variation, variation);
        this.speciesLifespan = randomizedLifespan;
    }

    public void BeEaten(float timeStep)
    {
        GetNutritionValue();
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

    public float GetNutritionValue()
    {
        /*Nutrition follows a bell curve (max in middle age)
        float peakAge = 0.5f;
        //width = how sharp or round the peak is
        float width = Mathf.Lerp(0.1f, 0.4f, Mathf.Clamp01(speciesLifespan / 100f)); // short-lived plants -> narrower curve, long-lived -> wider
        float gaussian = Mathf.Exp(-Mathf.Pow((ageFactor - peakAge) / width, 2)); //gaussian is simplified normal distribution
        nutritionValue = species.nutritionBaseValue * (0.5f + gaussian);*/

        float ageFactor = Mathf.Clamp01(age / speciesLifespan);
        float peakAge = 0.25f;
        float gaussianNew = Mathf.Exp(-Mathf.Pow((ageFactor - peakAge) / 0.25f, 2f));
        float gaussianPrev = Mathf.Exp(-Mathf.Pow(((Mathf.Max(ageFactor - WorldManager.Instance.timeStep)) - peakAge) / 0.25f, 2f));
        float newNutritionValue = species.nutritionBaseValue * (0.4f + gaussianNew);
        float previousNutritionValue = species.nutritionBaseValue * (0.4f + gaussianPrev);
        nutritionValue += (newNutritionValue - previousNutritionValue);
        return nutritionValue;
    }

    public virtual void Die()
    {
        isAlive = false;
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
}
