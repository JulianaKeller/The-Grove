using UnityEngine;

public class FightState : AnimalState
{
    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now fighting.");
        a.isFighting = true;
    }
    public override void Execute(Animal a, float dt) { }
    public override void Exit(Animal a) {
        a.isFighting = false;
    }
}
