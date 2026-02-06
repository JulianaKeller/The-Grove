using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class Plant : Entity
{
    public new PlantSpeciesData species;
    
    public float waterMeter;
    public float spreadChance;
    public bool canGrow;
    public float currentMoisture;
    public float currentFertility;

    public PlantView view;

    public Plant(PlantSpeciesData species, Vector3 position, int Id) : base(species, Id)
    {
        base.isAnimal = false;
        base.position = position;
        this.species = species;

        waterMeter = species.waterCapacity;
        spreadChance = 0f;
    }

    public void UpdatePlant(float timeStep)
    {
        // Get environment cell
        var coords = EnvironmentGrid.Instance.GetCellCoords(position);
        var cell = EnvironmentGrid.Instance.grid[coords.x, coords.y];

        currentFertility = cell.fertility;
        currentMoisture = cell.moisture;

        float maturity = Mathf.Clamp01(age / species.lifespan);

        //---Environmental checks---
        canGrow = true;
        prevSize = size;

        waterMeter -= species.waterNeed * timeStep * maturity;

        float refill = 2f * species.waterNeed * timeStep * maturity;

        if (currentMoisture > refill && waterMeter < species.waterCapacity)
        {
            waterMeter = Mathf.Min(waterMeter + refill, species.waterCapacity);
            cell.moisture = Mathf.Max(0, cell.moisture - refill);
        }

        if (currentFertility < species.minGroundFertility || waterMeter <= 0f)
        {
            health -= maxHealth * 0.1f; // lose health if soil too poor
            canGrow = false;
        }

        base.UpdateEntity(timeStep, canGrow);

        //---Growth ---
        if (canGrow)
        {
            health += maxHealth * 0.1f;
            health = Mathf.Min(health, maxHealth);

            float ageFactor = Mathf.Clamp01(age / speciesLifespan);

            spreadChance = species.baseSpreadChance * Mathf.Clamp01(ageFactor);

            if (Random.value < spreadChance)
            {
                Spread();
            }
        }

        if (age > speciesLifespan || health <= 0 /*|| GetNutritionValue() <= 0*/)
        {
            Die();
            Debug.Log(species.name + " died.");
            return;
        }
    }

    private void Spread()
    {
        int exp = 2; //Higher values should make closer spawning positions more likely
        float distance = species.spreadingRadius * Mathf.Pow(Random.value, exp);

        // Random direction around parent
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;

        Vector3 spawnPos = position + offset;

        // Check if position is inside environment bounds
        Vector2Int cell = EnvironmentGrid.Instance.GetCellCoords(spawnPos);
        if (cell.x < 0 || cell.x >= EnvironmentGrid.Instance.gridSize ||
            cell.y < 0 || cell.y >= EnvironmentGrid.Instance.gridSize)
        {
            return; // skip if outside world
        }

        PlantManager.Instance.SpawnPlant(species, spawnPos);
    }

    public override void Die()
    {
        base.Die();
        PlantManager.Instance.RemovePlant(this);
    }

    #region Overrides

    public static bool operator ==(Plant a, Plant b)
    {
        if (a is null || b is null) return false;

        return a.id == b.id;
    }

    public static bool operator !=(Plant a, Plant b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is not Plant other)
            return false;

        return id == other.id;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    #endregion
}
