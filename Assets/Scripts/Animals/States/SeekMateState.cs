using UnityEngine;

public class SeekMateState : AnimalState
{
    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now seeking a mate.");
    }
    public override void Execute(Animal a, float dt) { }
    public override void Exit(Animal a) { }
}
