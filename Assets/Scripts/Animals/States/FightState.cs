using UnityEngine;

public class FightState : AnimalState
{
    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now fighting.");
        a.isFighting = true;
    }
    public override void Execute(Animal a, float dt) {
        //Fighting either in defense or to get food
        //Fight or flight should be evaulated in either case
        //Fighting should otherwise be done until the target is dead
    }
    public override void Exit(Animal a) {
        a.isFighting = false;
    }
}
