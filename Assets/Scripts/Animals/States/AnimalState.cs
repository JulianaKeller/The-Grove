using UnityEngine;

public abstract class AnimalState
{
    public abstract void Enter(Animal a);
    public abstract void Execute(Animal a, float dt);
    public abstract void Exit(Animal a);
}
