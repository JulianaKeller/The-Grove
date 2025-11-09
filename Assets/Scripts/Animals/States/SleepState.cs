using UnityEngine;

public class SleepState : AnimalState
{
    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now sleeping.");
    }
    public override void Execute(Animal a, float dt) {
        //regenerate health
        //
    }
    public override void Exit(Animal a) { }
}
