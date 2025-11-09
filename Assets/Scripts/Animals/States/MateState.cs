using UnityEngine;

public class MateState : AnimalState
{
    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now mating.");
    }
    public override void Execute(Animal a, float dt) { }
    public override void Exit(Animal a) {
        a.matingDrive = 0;
        a.isMating = false;
    }
}
