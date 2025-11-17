using UnityEngine;

public class FollowState : AnimalState
{
    private Animal target;

    private float minDistance = 2f;

    public FollowState(Animal target)
    {
        this.target = target;
    }

    public override void Enter(Animal a) { }
    public override void Execute(Animal a, float timeStep) {
        //a.MoveTo(target)
        //Keep minimum distance from target

        if (target == null)
        {
            a.ChangeState(new IdleState());
            return;
        }

        Vector3 moveDir = (target.position - a.position).normalized;

        a.MoveTo(target.position - moveDir * minDistance, timeStep);
    }
    public override void Exit(Animal a) { }
}
