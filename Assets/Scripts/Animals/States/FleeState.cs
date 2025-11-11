using UnityEngine;

public class FleeState : AnimalState
{
    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now fleeing.");
        a.isFleeing = true;
    }
    public override void Execute(Animal a, float dt) { }
    public override void Exit(Animal a) {
        a.isFleeing = false;
    }
}
