using UnityEngine;

public class SeekWaterState : AnimalState
{
    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now seeking water.");
    }
    public override void Execute(Animal a, float dt) { }
    public override void Exit(Animal a) { }
}
