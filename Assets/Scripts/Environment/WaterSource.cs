using UnityEngine;

public class WaterSource
{
    public Vector3 position;

    public WaterSourceView view;

    public float radius;

    public float capacity;
    public float currentWater;

    public float evaporationRate = 0.01f;
    public float influenceRadius = 5f;
    public float fertilityBonus = 0.05f;
    public float moistureBonus = 0.05f;

    public WaterSource(Vector3 pos, float capacity)
    {
        position = pos;
        radius = 1f;
        this.capacity = capacity;
        currentWater = capacity;
    }

    public float Drink(float amount)
    {
        float previousWater = currentWater;
        currentWater = Mathf.Max(0f, currentWater - amount);
        float finalAmount = previousWater - currentWater;
        return finalAmount;
    }

    public void Refill(float amount)
    {
        currentWater = Mathf.Min(capacity, currentWater + amount);
    }

    public void UpdateWaterSource(float timeStep)
    {
        currentWater -= evaporationRate * timeStep;
        currentWater = Mathf.Clamp(currentWater, 0f, capacity);
    }

    public void RegisterInGrid()
    {
        EnvironmentGrid.Instance.RegisterWaterSource(this);
    }
}
