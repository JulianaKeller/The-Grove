using UnityEngine;

public class SeekMateState : AnimalState
{
    private Animal mate;

    private const float mateRange = 1.2f;

    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now seeking a mate.");
    }
    public override void Execute(Animal a, float timeStep) {
        Vector3 offset = mate.position - a.position;
        float dist = offset.magnitude;

        if (dist > mateRange)
        {
            a.MoveTo(mate.position, timeStep);
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
