using UnityEngine;

public class DrinkState : AnimalState
{
    private WaterSource waterSource;

    public DrinkState(WaterSource ws)
    {
        waterSource = ws;
    }

    public override void Enter(Animal a) {
        a.isDrinking = true;
    }
    public override void Execute(Animal a, float timeStep) {
        if(waterSource == null || waterSource.currentWater < timeStep)
        {
            a.ChangeState(new WanderState());
            return;
        }

        waterSource.DrinkFrom(timeStep);

        a.thirst = Mathf.Max(0, a.thirst - 0.2f * timeStep);

        if (a.thirst <= 0f)
        {
            a.ChangeState(new IdleState());
        }
    }

    public override void Exit(Animal a) {
        a.isDrinking = false;
    }

}
