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
    public Vector3 maxSize;
    public Vector3 minSize = new Vector3(0.01f, 0.01f, 0.01f);
    public float health;
    public float speciesLifespan;
    public float waterMeter;
    public float spreadChance;
    public float nutritionValue;
    public bool canGrow;
    public float currentMoisture;
    public float currentFertility;

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
        size = minSize;
        prevSize = minSize;
        health = 100;
        waterMeter = species.waterCapacity;
        spreadChance = 0f;
        nutritionValue = 0f;

        Debug.Log("Animal created.");
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

        //cell.fertility -= species.groundFertilityUsage * timeStep; //done in EnvironmentGrid currently
        //cell.fertility = Mathf.Max(0, cell.fertility);

        float refill = 2f * species.waterNeed * timeStep * maturity;

        if (currentMoisture > refill && waterMeter < species.waterCapacity)
        {
            waterMeter = Mathf.Min(waterMeter + refill, species.waterCapacity);
            cell.moisture = Mathf.Max(0, cell.moisture - refill);
        }

        if (currentFertility < species.minGroundFertility || waterMeter <= 0f)
        {
            health -= species.maxHP * 0.1f; // lose health if soil too poor
            canGrow = false;
        }

        //---Growth ---
        if (canGrow)
        {
            age += timeStep;
            health += species.maxHP * 0.1f;
            health = Mathf.Min(health, species.maxHP);

            // Normalized age 0–1
            float ageFactor = Mathf.Clamp01(age / speciesLifespan);

            //---Age-based values---

            /*float growthFactor = 1f / (1f + Mathf.Exp(-10f * (normalizedAge - 0.5f))); 
            float remaining = 1f - (size.x / species.maxSize.x);
            float growth = species.growthRate * growthFactor * remaining * timeStep;
            */

            float growth = species.growthRate * timeStep;
            size += Vector3.one * growth; //ToDo Use a more natural, nonlinear growth pattern based on plant age
            size.x = Mathf.Clamp(size.x, 0.01f, maxSize.x);
            size.y = Mathf.Clamp(size.y, 0.01f, maxSize.y);
            size.z = Mathf.Clamp(size.z, 0.01f, maxSize.z);

            // Nutrition follows a bell curve (max in middle age)
            float peakAge = 0.5f;
            //width = how sharp or round the peak is
            float width = Mathf.Lerp(0.1f, 0.4f, Mathf.Clamp01(speciesLifespan / 100f)); // short-lived plants -> narrower curve, long-lived -> wider
            float gaussian = Mathf.Exp(-Mathf.Pow((ageFactor - peakAge) / width, 2)); //gaussian is simplified normal distribution

            nutritionValue = species.nutritionBaseValue * (0.5f + gaussian);

            spreadChance = species.baseSpreadChance * Mathf.Clamp01(ageFactor);

            if (Random.value < spreadChance)
            {
                Spread();
            }
        }

        if (age > speciesLifespan || health <= 0)
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
}
