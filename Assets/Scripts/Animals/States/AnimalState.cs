using UnityEngine;

public abstract class AnimalState
{
    public abstract void Enter(Animal a);
    public abstract void Execute(Animal a, float dt);
    public abstract void Exit(Animal a);
    public static bool operator ==(AnimalState a, AnimalState b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;

        return a.GetType() == b.GetType();
    }

    public static bool operator !=(AnimalState a, AnimalState b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj is AnimalState other)
            return this.GetType() == other.GetType();
        return false;
    }

    public override int GetHashCode()
    {
        return GetType().GetHashCode();
    }
}
