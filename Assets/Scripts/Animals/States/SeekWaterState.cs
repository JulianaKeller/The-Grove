using UnityEngine;

public class SeekWaterState : AnimalState
{
    private WaterSource targetWaterSource;

    public override void Enter(Animal a) {
        targetWaterSource = a.FindNearestWaterSource();
        a.SetMoveTarget(targetWaterSource != null ? targetWaterSource.position : a.position);
        //ToDo offset of water source radius
    }

    public override void Execute(Animal a, float timeStep) {
        if (targetWaterSource == null)
        {
            a.ChangeState(new WanderState());
            return;
        }

        if (!a.hasPath)
        {
            a.ChangeState(new DrinkState(targetWaterSource));
            return;
        }
    }
    public override void Exit(Animal a) { }
}
