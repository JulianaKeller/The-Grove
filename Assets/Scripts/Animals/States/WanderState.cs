using UnityEngine;
using UnityEngine.UIElements;

public class WanderState : AnimalState
{
    public override void Enter(Animal a) { }
    public override void Execute(Animal a, float timeStep) {
        a.prevPosition = a.position;
        //a.position +=  ;
    }
    public override void Exit(Animal a) { }
}
