using UnityEngine;

public class SleepState : AnimalState
{
    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now sleeping.");
        a.isSleeping = true;
    }
    public override void Execute(Animal a, float dt) {
        //regenerate health
        //
    }
    public override void Exit(Animal a) {
        a.isSleeping = false;
    }
}
