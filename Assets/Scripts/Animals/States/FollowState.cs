using UnityEngine;

public class FollowState : AnimalState
{
    private Animal target;

    private float minDistance = 2f;

    public FollowState(Animal target)
    {
        this.target = target;
    }

    public override void Enter(Animal a) {

    }
    public override void Execute(Animal a, float timeStep) {

        if (target == null)
        {
            a.ChangeState(new IdleState());
            return;
        }

        Vector3 toTarget = target.position - a.position;
        float sqrDistance = toTarget.sqrMagnitude;

        if (sqrDistance <= a.followDistance * a.followDistance)
        {
            a.ChangeState(new IdleState());
            return;
        }

        a.SetMoveTarget(target.position);
    }

    public override void Exit(Animal a) { }
}
