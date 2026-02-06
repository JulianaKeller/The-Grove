using UnityEngine;

public class WaterSource
{
    public int id { get; private set; }
    public Vector3 center { get; private set; }

    public float baseCapacity = 100f;

    public WaterSourceView view;

    public int cellRadius;

    public float capacity;
    public float currentWater;

    public WaterSource(int Id, int cellRadius, Vector3 center)
    {
        id = Id;
        this.cellRadius = cellRadius;
        capacity = baseCapacity;
        currentWater = capacity;
        this.center = center;
    }

    public float DrinkFrom(float amount)
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

    public int GetWorldRadius()
    {
        return Mathf.CeilToInt(cellRadius * EnvironmentGrid.Instance.cellSize);
    }

    public float GetCellRadius()
    {
        return cellRadius;
    }

    public static bool operator ==(WaterSource a, WaterSource b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.id == b.id;
    }

    public static bool operator !=(WaterSource a, WaterSource b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null) return false;

        if (obj is WaterSource other)
            return id == other.id;

        return false;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }
}
