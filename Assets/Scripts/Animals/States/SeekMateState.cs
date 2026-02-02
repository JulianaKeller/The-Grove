using UnityEngine;

public class SeekMateState : AnimalState
{
    private Animal mate;

    private const float mateRange = 1.2f;

    public override void Enter(Animal a) {
        mate = a.GetNearbyMate();
        a.SetMoveTarget(mate != null ? mate.position : a.position);
        //ToDo offset to target
    }

    public override void Execute(Animal a, float timeStep) {
        if(mate == null)
        {
            a.ChangeState(new WanderState());
            return;
        }

        if (a.hasPath)
        {
            return;
        }
        else if (!mate.isMating && mate.matingDrive >= 0.5f)
        {
            mate.ChangeState(new MateState(a));
            a.ChangeState(new MateState(mate));
        }
        else
        {
            a.ChangeState(new FollowState(mate));
        }
    }
    public override void Exit(Animal a) { }
}
