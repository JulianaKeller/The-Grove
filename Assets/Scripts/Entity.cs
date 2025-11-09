using UnityEngine;

public abstract class Entity
{
    public int id;
    public Vector3 position; //current position
    public bool isAlive = true;

    public virtual void UpdateEntity(float dt) { }

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
