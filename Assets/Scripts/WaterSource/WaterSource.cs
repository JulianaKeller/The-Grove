using UnityEngine;

public class WaterSource
{
    public int id { get; private set; }

    public float baseCapacity = 100f;

    public Vector3 position;

    public WaterSourceView view;

    public float radius;

    public float capacity;
    public float currentWater;

    public WaterSource(Vector3 pos, int Id, float radius)
    {
        id = Id;
        position = pos;
        this.radius = radius;
        float area = Mathf.PI * radius * radius;
        capacity = baseCapacity * area;
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

    public void UpdateWaterSource(float timeStep, float evaporationRate)
    {
        currentWater -= evaporationRate * timeStep;
        currentWater = Mathf.Clamp(currentWater, 0f, capacity);
    }

    public static bool operator ==(WaterSource a, WaterSource b)
    {
        if (a is null || b is null) return false;

        return a.id == b.id;
    }

    public static bool operator !=(WaterSource a, WaterSource b)
    {
        return !(a == b);
    }
}
